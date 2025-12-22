# The 2024 atestat

The *atestat profesional* is the practical examination Romanian pupils sit at the end of the
last year of high school. In informatics it comes in two parts. There is a written and practical
programming paper, and there is a software project that you build, document, and then defend in
front of a committee of examiners. I sat it in July 2024 and both parts were marked 10 out of
10.

The project I defended was a resource management application for business use. It was written in
C# on Windows frameworks, it kept its data in a SQL database, and the client part was plain HTML,
CSS and JavaScript.

**That source is not published, and this repository is not it.** This is the same problem built
again during my first year at university. The marks above belong to the 2024 project and to the
defence, not to the code in this repository.

## What I would say differently now

The committee asked me to demonstrate the application, walk through the schema, and explain how
I stopped two people booking the same room. I answered the third question by showing them the
check in the C# code, and nobody asked what happens if two people click at the same moment.

Four things I would not do again, in the order they matter.

**The overlap check was a read followed by a write.** The application queried for a clash and
then inserted if it found none. Two requests arriving together both see a free room, and both
get it. That is the reason [ADR 2](adr/0002-the-database-decides-if-two-bookings-clash.md)
exists and it is the only interesting piece of engineering in the rebuild.

**There were no tests at all.** I demonstrated it by clicking through it the evening before. It
worked because I only ever did the things it could do, in the order it expected. I could not
have changed anything in that codebase safely, and by the defence I was afraid to.

**The connection string was in the source.** Server, database, user and password, sitting in the
file and printed in the documentation I handed in. It was a school laptop and a database on the
same machine, so nothing came of it, but I would not write it that way now.

**The schema was one `CREATE TABLE` script.** Every time I changed a column I dropped the
database and ran it again, which is fine while nothing is in it and useless afterwards. The
rebuild has migrations, so the schema has a history and the constraint that matters arrives in a
migration of its own.

The domain has not changed. An organisation has rooms, vehicles and equipment, people reserve
them, and two confirmed reservations for one item may never overlap. It was a good problem to
pick at eighteen and I picked it again rather than looking for a new one.
