# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Notable Version History

### [v9.0.2] - 2025-02-22
#### Changed
- Upgrade NATS.Net client library dependency (#283)
- MultiTenant entities - Get tenant from context only if there is any modified multitenant entity (#281)

### [v7.0.12] - 2023-10-19
#### Added
- **JetStream messaging transport** - Added NBB.Messaging.JetStream package (#265)
  - Initial implementation of NATS JetStream messaging transport
  - JetStream was developed with the following goals:
    - Easy to configure and operate with observability
    - Secure and compatible with NATS 2.0 security models
    - Horizontally scalable with high ingestion rate support
    - Support for multiple use cases
    - Self-healing and always available
    - API closer to core NATS
    - Allow NATS messages to be part of a stream
    - Payload agnostic behavior
    - No third-party dependencies

## Release Links

- [v9.0.2](https://github.com/osstotalsoft/nbb/releases/tag/v9.0.2)
- [v7.0.12](https://github.com/osstotalsoft/nbb/releases/tag/v7.0.12)
