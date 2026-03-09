# MessageBrokerStrategies Performance Testing

This project is designed to evaluate and compare the performance of two popular message broker solutions: **RabbitMQ** and **Kafka**. The goal is to provide insights into their throughput, latency, and reliability under various workloads, helping teams select the most suitable broker for their distributed systems.

## Project Purpose

- Benchmark and analyze the performance characteristics of RabbitMQ and Kafka.
- Demonstrate integration strategies for each broker within .NET 9 applications.
- Highlight the strengths and trade-offs of each solution.

## Message Broker Solutions

### RabbitMQ

**Advantages:**
- Implements the Advanced Message Queuing Protocol (AMQP), supporting reliable messaging and flexible routing.
- Easy to set up and manage, with a user-friendly management interface.
- Strong support for message acknowledgments, delivery guarantees, and complex routing patterns (topics, direct, fanout).
- Suitable for scenarios requiring transactional messaging and interoperability.

**Protocol Used:**  
- **AMQP (Advanced Message Queuing Protocol)**

### Kafka

**Advantages:**
- High throughput and scalability, designed for handling large volumes of data streams.
- Distributed, fault-tolerant architecture with built-in replication and partitioning.
- Excellent for event sourcing, real-time analytics, and log aggregation.
- Guarantees message durability and supports replaying messages.

**Protocol Used:**  
- **Kafka’s native protocol (TCP-based, optimized for streaming and partitioned data)**

---

## RabbitMQ Usage Guide

### Prerequisites 

RabbitMQ must be installed and running. Environment is Development
- URL: http://127.0.0.1:15672/
- User: guest
- Password: guest

**RabbitMQ Installation:**
- Download and install RabbitMQ from [https://www.rabbitmq.com/download.html](https://www.rabbitmq.com/download.html)

To use RabbitMQ in this project:

1. **Configure Exchanges in RabbitMessageBroker `appsettings.json`.**
   - Define exchanges in Rabbit.Exchanges.Name.

2. **Start RabbitMessageBroker first.**
   - This service must be running before any consumers connect.

3. **Configure Queues in RabbitMessageConsumer `appsettings.json`.**
   - Define queues in Rabbit.Exchanges.Queue.Name.

4. **Run RabbitMessageConsumer.**
   - The consumer binds the queue to an exchange, if the queue doesn't exist the queue is created.

5. **Test publishing messages to an exchange:**
   - Example endpoint:
     - `https://localhost:7277/rabbit/publish/exchange/Test-Exchange-1`
   - Example payload:
     ```json
     {
       "message": "This message is from Postman",
       "priority": 10
     }
     ```

5. **Message Priority Behavior:**
   - Try publishing messages while RabbitMessageConsumer is down.
   - - Example payload 1:
     ```json
     {
       "message": "This message is from Postman",
       "priority": 5
     }
     ```
   - - Example payload 2:
     ```json
     {
       "message": "This message is from Postman",
       "priority": 10
     }
     ```
   - When RabbitMessageConsumer is started, messages with higher priority are received first.

---

## Kafka Usage Guide

### Prerequisites

Kafka is a standalone product that runs on the Java Virtual Machine (JVM). Before using Kafka in this project, you must start a Kafka broker outside the .NET solution.

**Kafka Installation:**
- Download and install Apache Kafka from [https://kafka.apache.org/downloads](https://kafka.apache.org/downloads)

**To start Kafka on Windows:**
```
C:\kafka_2.13-4.2.0\bin\windows\kafka-server-start.bat C:\kafka_2.13-4.2.0\config\server.properties
```
Ensure you have Java installed and the Kafka distribution extracted to the specified path.

---

### Setting Topics and Partitions

Kafka organizes messages into **topics**, which are further divided into **partitions**. Topics allow you to categorize messages, while partitions enable parallelism and scalability. More partitions allow higher throughput and distributed processing, but require careful management of ordering and consumer load.

**In the KafkaMessageBroker app:**
- The topic name and partition count are configured in `appsettings.json`.
- Setting the topic and partition count is crucial for performance and scalability. Each partition can be processed independently, allowing multiple consumers to read from the same topic in parallel.

---

### Subscribing to a Topic in KafkaMessageConsumer

The `KafkaMessageConsumer` subscribes to a topic specified in its configuration. Subscription is handled in code via:
```
consumer.Subscribe(_config.GetSection("Kafka:Topic").Value);
```
**Key configuration options:**
- **AutoOffsetReset**: Determines where the consumer starts reading if no offset is present.  
  - `Earliest`: Reads all available messages from the beginning.
  - `Latest`: Reads only new messages arriving after subscription.
- **GroupId**: Consumers with the same group ID share the workload for a topic. Each message is delivered to one consumer in the group. Changing the group ID will cause the consumer to start from the offset defined by `AutoOffsetReset`.
- **Topic**: Must match the topic created in KafkaMessageBroker and set in `appsettings.json`.

**Caveats:**
- If `AutoOffsetReset` is set to `Earliest`, the consumer will process all messages in the topic from the beginning if no prior offset exists for the group.
- If `EnableAutoCommit` is true, offsets are committed automatically, which can affect message replay and recovery.
- Changing the `GroupId` or subscribing to a new topic will reset the offset behavior according to `AutoOffsetReset`.
- Ensure the topic exists and matches the configuration, otherwise the consumer will fail to subscribe.

---

### Example Workflow

1. **Start Kafka broker** as described above.
2. **Configure topic and partitions** in KafkaMessageBroker `appsettings.json`.
3. **Run KafkaMessageBroker** to create the topic.
4. **Configure topic and group ID** in KafkaMessageConsumer `appsettings.json`.
5. **Run KafkaMessageConsumer** to subscribe and process messages.

---

## Feature Comparison: Kafka vs RabbitMQ (Pub/Sub)

| Feature                | RabbitMQ (Pub/Sub)                                  | Kafka (Pub/Sub)                                      |
|------------------------|-----------------------------------------------------|------------------------------------------------------|
| Protocol               | AMQP                                                | Kafka TCP protocol                                   |
| Message Durability     | Persistent queues, configurable per message/queue   | Always persistent, log-based storage                 |
| Ordering Guarantees    | Per queue, can be affected by consumer concurrency  | Per partition, strict ordering within partition      |
| Scalability            | Scales with exchanges/queues, clustering supported  | Scales with partitions, distributed by design        |
| Consumer Model         | Push-based, consumers receive messages as delivered | Pull-based, consumers fetch messages on demand       |
| Replay/Recovery        | Limited, requires dead-letter or manual handling    | Built-in, consumers can replay from any offset       |
| Message Acknowledgment | Manual or automatic, supports retries               | Offset commit, manual or automatic                   |
| Routing Flexibility    | Exchanges (direct, topic, fanout, headers)          | Topics only, no built-in routing                     |
| Management UI          | Web-based management interface                      | No official UI, third-party tools (e.g., Confluent)  |
| Transaction Support    | Yes, supports transactions                          | Limited, idempotency via producer/consumer configs   |
| Delivery Guarantees    | At-most-once, at-least-once, exactly-once (with tx) | At-most-once, at-least-once, exactly-once (with tx) |
| Consumer Groups        | Not native, can be emulated with queues             | Native, enables parallel processing                  |
| Message TTL            | Supported                                           | Not natively supported                               |
| Use Cases              | Task queues, transactional messaging, RPC           | Event streaming, log aggregation, analytics          |

---
