# ExpressWalker
ExpressWalker provides a generic way to examine and change any object graph in fashion similar to "Visitor Pattern".
One can build generic hierarchy composition - called visitor - capable to "visit" any property, collect its value and change it.
The things that are configurable are: property owner, property name & type, depth of visit, changing value expression, cloning...
Uses refleciton only while building re-usable visitor (initial step) and relies purely on expression trees afterwards.
It is optionally protected from circular references. It provides fluent API in the following way:

//example of building a visitor and visiting an object (with cloning it, specifying visit depth and protecting circular reference):

```

var visitor = TypeWalker<Parent>.Create()
                  .ForProperty<Parent, string>(p => p.TestString1, null, (old, met) => old + met)
                  .ForProperty<Child, DateTime>(p => p.TestDate1, null, (old, met) => old.AddYears(10))
                  .ForProperty<CommonType>(null, (old, met) => new CommonType { CommonString = "..." });
                  
  var parentClone = new Parent();
  visitor.Visit(parentObject, parentClone, 10, new InstanceGuard()); 

// 1. 'TestString1' property of Parent objects, anywhere in Parent's hierarchy
// 2. 'TestDate1' property of Child objects, anywhere in Parent's hierarchy
// 3.  Any property of type CommonType, anywhere in Parent's hierarchy

//Property setter: new value for property 'TestString1' is set by compiled expression '(old, met) => old + met'
//'old' is old value, 'met' is metadata obj. coming from [VisitorMetadata] attribute on 'TestString1' property.

```

