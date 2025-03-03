using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Assertions;
using Object = System.Object;

// Forked from https://github.com/tsepton/ummi
public class AttributeParser {
	private readonly RegisteredMethod[] _methods;

	public RegisteredMethod[] Methods => _methods;
	public RegisteredMethod[] AbstractMethods => _methods.Where(m => m.Info.IsAbstract).ToArray();
	public RegisteredMethod[] StaticMethods => _methods.Where(m => m.Info.IsStatic).ToArray();
	public RegisteredMethod[] ConcreteMethods => _methods.Where(m => !m.Info.IsAbstract).ToArray();

	public AttributeParser(Type[] classes) {
		var extractedMethods = TypeCache
		  .GetMethodsWithAttribute<UserAction>()
		  .Where(method => classes.Contains(method.DeclaringType));

		List<RegisteredMethod> methods = new List<RegisteredMethod>();
		var extracted = extractedMethods
		  .Where(m => !m.IsPrivate)
		  .Where(m => m.ReturnType == typeof(void))
		  .Where(m => !m.IsAbstract)
		  .Where(m => m.IsStatic); // Future work
		foreach (var method in extracted) {
			string[] utters = method.GetCustomAttribute<UserAction>().Utterances;
			methods.Add(new RegisteredMethod(method, utters));
		}

		_methods = methods.ToArray();
	}

	public class RegisteredMethod {
		public MethodInfo Info { get; }
		public double[][] Embeddings { get; }
		public string[] Utters { get; }

		public RegisteredMethod(MethodInfo method, string[] utters) {
			Info = method;
			Utters = utters;
		}

		public int GetNumberOfParameters() {
			return Info.GetParameters().Length;
		}

		public void Invoke() {
			Invoke(new object[] { });
		}

		public void Invoke(Object[] args) {
			Assert.AreEqual(args.Length, GetNumberOfParameters());
			Info.Invoke(null, args);
		}

		public override string ToString() {
			return $"Registered MMI Method called {Info.Name}";
		}
	}
}