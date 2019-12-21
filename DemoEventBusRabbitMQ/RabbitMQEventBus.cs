using DemoEventBusFramewrok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Autofac;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace DemoEventBusRabbitMQ
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly ILifetimeScope _autofac;
        
        private readonly int _retryCount;

        private IModel _consumerChannel;
        private string _queueName;

        public RabbitMQEventBus(IRabbitMQPersistentConnection persistentConnection,
            ILifetimeScope autofac,
            IEventBusSubscriptionsManager subsManager,
            string queueName = null, int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _queueName = queueName;
            _consumerChannel = CreateConsumerChannel();
            _autofac = autofac;
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }



        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                   
                });

            var eventName = @event.GetType().Name;

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: EventBusConstants.BROKER_NAME, type: "direct");

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;

                    channel.BasicPublish(
                        exchange: EventBusConstants.BROKER_NAME,
                        routingKey: eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);
                });
            }

            return Task.CompletedTask;
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _subsManager.GetEventKey<TEvent>();
            DoInternalSubscription(eventName);

           

            _subsManager.AddSubscription<TEvent, TEventHandler>();
            StartBasicConsume();
        }

        public void Unsubscribe<TEvent, TEventHandler>()
           where TEvent : IEvent

           where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _subsManager.GetEventKey<TEvent>();


            _subsManager.RemoveSubscription<TEvent, TEventHandler>();
        }

        private void StartBasicConsume()
        {

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
               
            }
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: _queueName,
                                      exchange: EventBusConstants.BROKER_NAME,
                                      routingKey: eventName);
                }
            }
        }

        private async Task  Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body);

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
              
            }

            //即使出现异常也要关闭队列，因为在真实环境中可能会产生死信队列。
            //更多信息查看: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }


            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: EventBusConstants.BROKER_NAME,
                                     type: "direct");

            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.CallbackException += (sender, ea) =>
            {
               
                _consumerChannel.Dispose();
                //_consumerChannel = CreateConsumerChannel();
                try
                {
                    if (_consumerChannel.ChannelNumber > 0)
                    {
                        StartBasicConsume();
                    }
                }
                finally {
                    _consumerChannel = CreateConsumerChannel();
                }
            };
           
            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _autofac.BeginLifetimeScope(EventBusConstants.AUTOFAC_SCOPE_NAME))
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {

                        var handler = scope.ResolveOptional(subscription) as IEventHandler;
                        if (handler == null) continue;
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                        await Task.Yield();
                        await (Task)concreteType.GetMethod("HandleAsync").Invoke(handler, new object[] { integrationEvent });

                    }
                }
            }
            else
            {
               
            }
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: EventBusConstants.BROKER_NAME,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _subsManager.Clear();
        }
    }
}
