# export
Export 2.0

An evolution of the Three Rings 'export' serialization system for Java and C#.

(Work in progress! None of this works yet. Not actively being developed presently.)

GOALS
- A larger set of built-in types including standard Collection types.
- Full type information is transmitted, including generics.
- Able to pass-over an entire type if it's unknown when importing.
  The stream's metadata is used exclusively; local types are used
  as data containers but cannot affect how the stream is read.
- "stretch goal", but sorta the key to the whole thing: be able to
  convert between XML and DAT formats *without access to the classes
  that were exported*.

NOTE PILE

 - Don't use modified UTF-8. (use UTF-8 with a real varlong length prefix)
 - prior to actually constructing a class, make sure it's in the whitelist or exportable
 - be able to pass-over reading an unknown class
 - Basic understanding of collection types, and not write out that something is a "HashMap".
   - but still need to support ArgumentMap (maybe it implements an interface?) Exportable...
 - Array & Collection types expect follow-on generic type arguments.
 - How are new generic classes defined?
   - when sending the class, the flags can encode how many generic args there are..
   - define a new class List<T> that uses List and T...
 - multi-dimensional arrays are {array of {array of {int}}}
 - when importing, details on read-in types are localized. One stream's "FooClass" might
   be completely different from another stream's FooClass.

- Why are fields identified by type & name? Wtf? Simplify if possible.
   - two kinds of data with a class: fields and values.
     - fields are given an id, and when first seen their name and type are transmitted.
     - values are identified by a string each time, written by custom export methods.
     - values are written by custom exporters and whether or not a class even transmits values
       is encoded in the flags. Most classes won't.
     - On the importer side, reading values can "see" fields. If a value is not found,
       then fields are checked by-name.
     - And if a field is unused, it can see through to the values and possibly use those???
     - On the reader side, you can read fields as values by their name. But can something
       written as a value be read into a field if it's compatible? A question...
       - Originally I had this idea whereby each layer of the class hierarchy would
         only see fields/values at that layer.
         So B extends A, C extends B. If B has a static export() method then A and C would
         still have their fields written the standard way. But then, don't I need to separate
         out the fields for each class?
       - Similarly: how do we want to handle shadowed fields?
         - During examination of an Exportable class, see that two fields have the same name
           and warn that the later one will be skipped.
     - Got to handle: on the Java side, if we read-in a List<Integer> but we need to shove
       it into a List<Long> that will *work* but is incorrect! We'll need to note that
       the types don't match... Shit.

 - Another problem with current exporting: on the Java side, Number types are always coerced
   even if it causes a loss of precision! Now under the new code, primitive number types
   only fit if they actually fit. (smaller being put into bigger)

 - IDEALLY, we would have an XML <-> DAT converter that needs ZERO access to the classes
   described therein.

 - Fix type erasure: object id references are specific to their field type?
   What about bare types in e.g. Map? shit.
 - collections arrive as immutable? No... but we need to support the immutable types, right?
   - yes
 - be able to send a "new object id" indicating that the object is "anonymous", one time
   only, so that the client doesn't need to stash it in a map.
