using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using RabbitMQ.Client;
using System.Collections;
using RabbitMQ.Client.Framing;
using RabbitMQ.Client.Events;

namespace Publisher
{
    public class Send
    {
        private static readonly string appID = ConfigurationManager.AppSettings["AppID"];
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { Uri = ConfigurationManager.AppSettings["RabbitMQUri"] };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    string queue = string.Format("MQ{0}.RpcTaskQueue", appID);
                    string replyQueueName;
                    channel.QueueDeclare(queue, false, false, false, null);//声明消息队列

                    replyQueueName = channel.QueueDeclare().QueueName;

                    //QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel);
                    //channel.BasicConsume(replyQueueName, true, consumer);


                    

                    

                    while (true)
                    {

                        string corrId = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        //设置replyTo和correlationId属性值  
                        IBasicProperties props = channel.CreateBasicProperties();
                        props.CorrelationId = corrId;
                        props.ReplyTo = replyQueueName;

                        var message = Console.ReadLine();
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish("", queue, props, body);   //发送消息

                        

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (s, e) =>
                        {
                            
                            var result = Encoding.UTF8.GetString(e.Body);
                            Console.WriteLine("消息回执： {0} ，CorrelationId:{1}", result, e.BasicProperties.CorrelationId);
                           // Console.Write("请输入要发送的消息：");
                        };
                        channel.BasicConsume(replyQueueName, true, consumer);    //开启消费者与通道、队列关联
                        Console.WriteLine("已发送的消息： {0},CorrelationId:{1}", message,corrId);
                    }



                }
            }

        }


    }
}
