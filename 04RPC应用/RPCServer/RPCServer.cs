using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace RPCServer
{
    public class RPCServer
    {
        private static readonly string appID = ConfigurationManager.AppSettings["AppID"];
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { Uri = ConfigurationManager.AppSettings["RabbitMQUri"] };
            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                string queue = string.Format("MQ{0}.RpcTaskQueue", appID);
                string replyQueueName;
                channel.QueueDeclare(queue, false, false, false, null);//声明消息队列
                channel.BasicQos(0, 1, false);
                Console.WriteLine("准备接收消息：");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (s, e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body);
                    SimulationTask(message, channel, e);
                    channel.BasicAck(e.DeliveryTag, false); //手动Ack：用来确认消息已经被消费完成了
                };
                channel.BasicConsume(queue, false, consumer);    //开启消费者与通道、队列关联

                Console.ReadLine();
            }



        }

        public static string StringBuild(string mes)
        {
            return mes + ":MQ_OK";
        }


        /// <summary>
        /// 模拟消息任务的处理过程
        /// </summary>
        /// <param name="message">消息</param>
        private static void SimulationTask(string message, IModel channel, BasicDeliverEventArgs e)
        {
            Console.WriteLine("接收的消息： {0}", message);
            int dots = message.Split('.').Length - 1;
            Thread.Sleep(dots * 1000);

            IBasicProperties basicProps = e.BasicProperties;
            IBasicProperties props = channel.CreateBasicProperties();
            props.CorrelationId = basicProps.CorrelationId;
            Console.WriteLine("已消费：【" + message + "】,已回执");
            var body = Encoding.UTF8.GetBytes(StringBuild(message));
            channel.BasicPublish("", basicProps.ReplyTo, props, body);
            Console.WriteLine("接收的消息处理完成，现在时间为{0}！", DateTime.Now);
        }
    }
}
