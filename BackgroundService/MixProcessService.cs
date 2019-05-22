using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using SignalRDemo.Hubs;
using Newtonsoft.Json;
using Confluent.Kafka;
using Api.KafkaUtil;
using System.Collections.Generic;
using System.Linq;

namespace Api.BackgroundServices
{
    public class MixProcessService : BackgroundService
    {
        public MixProcessService()
        {
        }
        public MixProcessService(IHubContext<OrderMonitorHub, IMessages> orderMonitorHub,ProducerConfig producerConfig, ConsumerConfig consumerConfig)
        {
            this._orderMonitorHub = orderMonitorHub;
            this._producerConfig = producerConfig;
            this._consumerConfig = consumerConfig;
        }
        private IHubContext<OrderMonitorHub, IMessages> _orderMonitorHub;
        private ProducerConfig _producerConfig;
        private ConsumerConfig _consumerConfig;

        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"Mix service is running at: {DateTime.Now}");

            //Get All active signalR connections 
           // var allConnections = new Dictionary<string,Consumer<string,string>>();
            

            while (!stoppingToken.IsCancellationRequested)
            {
                var allConnections = new Dictionary<string,Consumer<string,string>>(SignalRKafkaProxy.AllConsumers);
                
                //Logging this for debuggin purpose
                Console.WriteLine("Connections count:"+allConnections.Count);
                foreach (var c in allConnections)
                {
                    Console.WriteLine("ConnectionId: "+c.Key);
                }
                
                if(allConnections!=null){

                    //1. For each connected client, make one kafka connection and keep polling it. 
                    //2. When we recieve message from KAFKA on a particular connection, inform(notify) the specific client with the message.
                    
                    foreach(var connection in allConnections){
                        
                        //Read a message
                        string connectionId = connection.Key;
                        Console.WriteLine($"connection: {connectionId}, consumer:{connection.Value}");
                        
                        Consumer<string,string> consumerConnection = connection.Value;

                        var consumerResult = consumerConnection.Consume(new TimeSpan(0,0,15));

                        if(consumerResult!=null){

                            //Deserilaize 
                            OrderRequest orderRequest = JsonConvert.DeserializeObject<OrderRequest>(consumerResult.Value);
                            Console.WriteLine($"Info: Recieved order to mix. Id# {orderRequest.Id}");

                            //Step 1: If there is a new message in KAFKA "Orders" topic, inform the client.
                            Console.WriteLine($"Informing UI connected client - {connectionId} about the newly recieved order. Id# {orderRequest.Id}");

                            await _orderMonitorHub.Clients.Client(connectionId).InformNewOrderToMix(orderRequest);
                        }
                    }
                }
            }
        }
    }
}