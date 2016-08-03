# ExpressWalker
ExpressWalker provides a generic way to examine and change any object graph in fashion similar to "Visitor Pattern".
One can build generic hierarchy composition - called visitor - capable to "visit" any property, collect its value and change it.
The things that are configurable are: property owner, property name & type, depth of visit, changing value expression, cloning...
Uses refleciton only while building re-usable visitor (initial step) and relies purely on expression trees afterwards.
It is optionally protected from circular references. It provides fluent API:

