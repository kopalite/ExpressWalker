# ExpressWalker
ExpressWalker provides a generic way to examine and change any object graph in fashion similar to "Visitor Pattern".
You can build generic hierarchy composition (visitor) capable to "visit" and change any property, basing on configuration.
Uses refleciton only while building a visitor and relies purely on expression trees while visiting objects.

That's why **IT IS WAY FASTER** than custom solutions usually built with reflection.

It is optionally protected from circular references so you can avoid StackOverflowException easily.
Provides fluent API while building a visitor which increases code readability 
in terms of recognizing the hierarchy buing built right away from the code.
The optional and configurable things available are:

- visiting properties by matching owner type and property name 
- visiting properties by matching property type only
- specifying depth of visit in run-time (not during configuration)
- custom expression for changing property value 
- cloning of visited object
- etc.

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
in specific place in object graph (e.g. only a 'Child' element at level 3 and only it's 'TestString1' property).
