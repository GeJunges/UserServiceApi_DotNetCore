

/****IN CONSTRUCTION****/

STARTUP class:
	METHODS:
		Startup = This method is responsible for loading configuration values.
		ConfigureServices = This method gets called by the runtime. Use this method to add services to the container of dependency injector.
		Configure = This method gets called by the runtime. Use this method to configure and add Middlewares the HTTP request pipeline.

	DI ConfigureServices:
		• Instance: A specific instance is given all the time. You are responsible for its initial creation.
		• Transient: A new instance is created every time.
		• Singleton: A single instance is created and it acts like a singleton.
		• Scoped: A single instance is created inside the current scope. It is equivalent to Singleton in the current scope.


		