//using MappingGenerator.SampleApp;

//using Microsoft.Extensions.DependencyInjection;

//using Talk2Bits.MappingGenerator.Abstractions;

//var sc = new ServiceCollection();

//sc.RegisterMapper<AB>();
//sc.RegisterMapper<CD>();

////sc.AddSingleton<AB>();
////sc.AddSingleton<IMapper<A, B>>(p => p.GetRequiredService<AB>());

////sc.AddSingleton<CD>();
////sc.AddSingleton<IMapper<C, D>>(p => p.GetRequiredService<CD>());
////sc.AddSingleton<IMapper<IEnumerable<C>, D>>(p => p.GetRequiredService<CD>());

//var sp = sc.BuildServiceProvider();
//var mapper = sp.GetRequiredService<IMapper<IEnumerable<C>, List<D>>>();
//var x = sp.GetRequiredService<IMapper<C, D>>();
//var xx = (IMapper<IEnumerable<C>, IEnumerable<D>>)x;


//var source = new[]
//{
//    new C { Text = "1", Inner = new A { Text = "Inner1" } },
//    new C { Text = "2", Inner = new A { Text = "Inner2" } },
//};

//var qq = xx.Map(source);
//var result = mapper.Map(source);


Console.WriteLine("Hello, World!");