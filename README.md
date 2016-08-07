# ExpressWalker
ExpressWalker provides a generic way to examine and change any object graph in fashion similar to "Visitor Pattern". You can build generic hierarchy composition (visitor) capable to "visit" and change any object's property, basing on configuration. Uses reflection only while building a visitor and relies purely on expression trees while visiting objects.

That's why **IT IS WAY FASTER** than custom solutions usually built with reflection.

It is optionally protected from circular references so you can avoid StackOverflowException easily.
Provides fluent API while building a visitor which increases code readability 
in terms of recognizing the hierarchy being built right away from the code.
The optional and configurable things available are:

- visiting properties by matching owner type and property name 
- visiting properties by matching property type only
- specifying depth of visit in run-time (not during configuration)
- custom expression for changing property value 
- cloning of visited object
- etc.

```

//example 1 - IVisitor that visits properties by property names and/or types (start from TypeWalker class):

    var typeVisitor = TypeWalker<Parent>.Create()
						.ForProperty<Parent, string>(p => p.TestString1, (old, met) => old + met)
						.ForProperty<Child, DateTime>(p => p.TestDate1, (old, met) => old.AddYears(10))
						.ForProperty<CommonType>((old, met) => new CommonType { CommonString = "..." })
					.Build();

    var parentClone = new Parent();
    var propertyValues = new HashSet<PropertyValue>()

    typeVisitor.Visit(parentObject, parentClone, 10, new InstanceGuard(), propertyValues); 
  
//example 2 - IVisitor that visits properties by explicit configuration (start from ManualWalker class):

    var manualVisitor = ManualWalker.Create<A1>()
                                    .Property<A1, DateTime>(a1 => a1.A1Date, (va1, m) => va1.AddYears(10))
                                    .Element<A1, B1>(a1 => a1.B1, b1 =>
                                            b1.Property<B1, string>(x => x.B1Name, (vb1, m) => vb1 + "Test2"))
                                    .Element<A1, B2>(a1 => a1.B2, b2 => b2
                                            .Property<B2, DateTime>(x => x.B2Date, (vb2, m) => vb2.AddYears(10)))
                                .Build();

	var parentBlueprint = new A1();
	var values = new HashSet<PropertyValue>();
	manualVisitor.Visit(parentObject, parentBlueprint, 10, new InstanceGuard(), values);
			
//Paremeter 'met' in expressions above is optional metadata object set in design-time. 
//It can be set by [VisitorMetadata] property attribute in visited class.
//e.g. in example above, there is [VisitorMetadata("AnyString")] on property Parent.TestString1.
			
```

Many thanks to Francisco José Rey Gozalo for contributing with ideas and solutions.
