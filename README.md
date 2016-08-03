# ExpressWalker
ExpressWalker provides a generic way to examine and change any object graph in fashion similar to "Visitor Pattern".
One can build generic hierarchy composition - called visitor - capable to "visit" any property, collect its value and change it.
Uses refleciton only while building re-usable visitor (initial step) and relies purely on expression trees afterwards.
That's why **it is way much faster** than custom solutions that are usually built with reflection.
It is optionally protected from circular references. It provides fluent API in the following way:

The optional and configurable things available are

- owner type of visited property 
- name & type of visited property
- depth of visit 
- expression for changing property value 
- cloning...



```

//building a visitor + visiting an object (with optional clone, visit depth and circular reference protection):

var visitor = TypeWalker<Parent>.Create()
                  .ForProperty<Parent, string>(p => p.TestString1, null, (old, met) => old + met)
                  .ForProperty<Child, DateTime>(p => p.TestDate1, null, (old, met) => old.AddYears(10))
                  .ForProperty<CommonType>(null, (old, met) => new CommonType { CommonString = "..." })
                  .Build();
                  
  var parentClone = new Parent();
  visitor.Visit(parentObject, parentClone, 10, new InstanceGuard()); 

// 
// 1. 'TestString1' property of Parent objects, anywhere in Parent's hierarchy
// 2. 'TestDate1' property of Child objects, anywhere in Parent's hierarchy
// 3.  Any property of type CommonType, anywhere in Parent's hierarchy

//Property setter: new value for property 'TestString1' is set by compiled expression '(old, met) => old + met'
//'old' is old value, 'met' is metadata obj. coming from [VisitorMetadata] attribute on 'TestString1' property.

```
There is also an option of ManualWaler for building more specific visitor that will visit only nodes
in specific place in object graph (e.g. only a 'Child' element at level 3 and only it's 'TestString1' property.
