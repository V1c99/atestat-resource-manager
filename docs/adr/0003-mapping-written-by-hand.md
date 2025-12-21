# 3. Mapping written by hand

- Date: 2025-11-19
- Status: accepted

## Context

There are four entities and four response shapes, and something has to turn one into the other.
Every tutorial I read reached for AutoMapper without saying why.

I have one real reason to keep the mapping visible. A booking arrives from the browser with a
`+02:00` offset on it and has to be stored in UTC, and I wanted one obvious place where that
happens rather than a convention hidden in a profile.

## Decision

Extension methods in `Contracts/Mapping.cs`. `ToResponse` for each entity, `ToBooking` and
`ToResource` for the two requests that create something.

## Consequences

It is more lines than a profile would be, and every new field has to be added in two places.

In exchange, if I delete a property the build breaks instead of the response quietly losing a
field at runtime. I can search for `ToResponse` and read every mapping in one screen. And
`ToBooking` is the one place where `ToUniversalTime` is called.

At four entities this is clearly the right size. If this grew to forty I would think about it
again.
