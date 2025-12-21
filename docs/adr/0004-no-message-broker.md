# 4. No message broker

- Date: 2025-12-20
- Status: accepted

## Context

Nearly every .NET job advert I have read lists RabbitMQ or Kafka, and I thought about adding one
so that the repository would show it. The idea was to publish a `BookingCreated` message and
have something consume it.

## Decision

No broker. The API writes to Postgres and answers the request.

## Consequences

Nothing in this application is actually asynchronous. There is one database, one service and one
organisation's worth of bookings, and the only consumer I could invent for the message was a
notification email that the application does not send. Adding a broker would have meant running
another container so that a message could be published and read by nobody.

If the application ever did send confirmation emails, that is the point where a queue would earn
its place, because the email must not fail the booking and must survive a restart.

I would rather be asked why there is no broker and have this answer than be asked what the
broker is for and not have one.
