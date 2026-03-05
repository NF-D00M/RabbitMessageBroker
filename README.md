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

Explore the repository to review implementation details, performance benchmarks, and integration examples for both RabbitMQ and Kafka within .NET 9.