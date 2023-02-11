# genocs-basket
A step by step basket system. 

It use the awesome [MassTransit](https://masstransit-project.com/) library .NET7


## Prerequisite

Following componants are used along with the others.

- RabbitMQ
- MongoDB
- Redis
- [Jaeger](https://www.jaegertracing.io/) for tracing.
- [OpenTelemetry](https://opentelemetry.io/)

To setup the infrastructure components as Docker container, a compose file is provided.

To setup components using Enterprice Orchestrator please check [enterprise-containers](https://github.com/Genocs/enterprise-containers) repo.


## Setup
### Infrastructure
To setup infrastructure components, run following command

``` bash
docker compose -f ./infrastructure/containers/infrastructure.yml up -d
```

### Connect to Redis on container
``` bash
docker exec -it redis redis-cli
```



## Logging
[Serilog Datalust](https://blog.datalust.co/using-serilog-in-net-6/)