# 1. Postgres instead of SQL Server

- Date: 2025-11-09
- Status: accepted

## Context

The original version stored everything in SQL Server, because that is what the school lab had
installed and what the C# examples in the textbook used. It worked, but setting it up on a
different machine meant a page of instructions and about half an hour, and I had to write that
page for the defence.

I had also read that Postgres can express constraints that SQL Server cannot. At this point I
did not know that I was going to need one.

## Decision

PostgreSQL 17, reached through Npgsql and EF Core, running in a container that
`docker compose up` starts.

## Consequences

I gave up the tooling I already knew. SQL Server Management Studio is friendlier than psql and I
had used it for two years.

What I got back is a database that starts in a few seconds on any machine with Docker, and,
three weeks later, the exclusion constraint in
[ADR 2](0002-the-database-decides-if-two-bookings-clash.md). SQL Server has no equivalent, so
that decision turned out to depend on this one.

The part that cost me an evening is that `timestamptz` in Postgres will not accept a
`DateTimeOffset` whose offset is not zero. Npgsql throws before the query is even sent. Every
booking is converted to UTC in one place now, and there is a test for it.
