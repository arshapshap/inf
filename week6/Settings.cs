using System;

namespace HttpServer
{
	class Settings
	{
		public string Path { get; set; } = "./site";
		public int Port { get; set; } = 8080;
	}
}