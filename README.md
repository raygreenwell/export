# This project is dead.
I had fun but my needs changed before I finished noodling here.

You're welcome to peruse the archive...

---

---

---

## export
Export 2.0
==========

An evolution of the Three Rings 'export' serialization system for Java and C#.

(Work in progress! None of this works yet. Not actively being developed presently.)

Also: presently depends on ooo-specific library versions... TODO.

GOALS
-----
- A larger set of built-in types including standard Collection types.
- Full type information is transmitted, including generics.
- Able to pass-over an entire type if it's unknown when importing.
  The stream's metadata is used exclusively; local types are used
  as data containers but cannot affect how the stream is read.
- "stretch goal", but sorta the key to the whole thing: be able to
  convert between XML and DAT formats *without access to the classes
  that were exported*.
