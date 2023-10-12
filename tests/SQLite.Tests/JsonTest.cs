using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif

namespace SQLite.Tests
{
	[TestFixture]
	public class JsonTest
	{
		public class TestChildObject
		{
			public string TestChildString { get; set; }
			public int? TestChildInt { get; set; }
			public DateTime? TestChildDate { get; set; }

			public override bool Equals (object obj)
			{
				var typedObj = obj as TestChildObject;
				if (typedObj == null) return false;

				return string.Equals(TestChildString, typedObj.TestChildString)
					&& typedObj.TestChildInt == TestChildInt
					&& typedObj.TestChildDate == TestChildDate;
			}

			public override int GetHashCode ()
			{
				return HashCode.Combine (TestChildString, TestChildInt, TestChildDate);
			}
		}

		public class TestObj
		{
			[PrimaryKey]
			public string Id { get; set; }

			public IEnumerable<int> TestIntEnumerable { get; set; }
			public IEnumerable<string> TestStringEnumerable { get; set; }
			public IEnumerable<TestChildObject> TestObjectEnumerable { get; set; }
			public TestChildObject ChildObject { get; set; }
		}

		public class TestDb : SQLiteConnection
		{
			public TestDb (String path)
				: base (path)
			{
				CreateTable<TestObj> ();
			}
		}

		[Test]
		public void ShouldPersistAndReadChildObjects ()
		{
			var db = new TestDb (TestPath.GetTempFileName ());

			var obj1 = new TestObj () { Id = "1", ChildObject = new TestChildObject { TestChildInt = 1, TestChildDate = DateTime.Now.AddSeconds (1), TestChildString = "Test Child String 1" } };
			var obj2 = new TestObj () { Id = "2", ChildObject = new TestChildObject { TestChildInt = 2, TestChildDate = DateTime.Now.AddSeconds (2), TestChildString = "Test Child String 2" } };

			var numIn1 = db.Insert (obj1);
			var numIn2 = db.Insert (obj2);
			Assert.AreEqual (1, numIn1);
			Assert.AreEqual (1, numIn2);

			var result = db.Query<TestObj> ("select * from TestObj").ToList ();
			Assert.AreEqual (2, result.Count);
			Assert.IsNotNull (result[0].ChildObject);
			Assert.IsNotNull (result[1].ChildObject);
			Assert.AreEqual (obj1.ChildObject, result[0].ChildObject);
			Assert.AreEqual (obj2.ChildObject, result[1].ChildObject);

			Assert.AreEqual (obj1.Id, result[0].Id);
			Assert.AreEqual (obj2.Id, result[1].Id);

			db.Close ();
		}

		[Test]
		public void ShouldPersistAndReadIntEnumerables ()
		{
			var db = new TestDb (TestPath.GetTempFileName ());

			var obj1 = new TestObj () { Id = "1", TestIntEnumerable = new List<int> { 1, 2, 3 } };
			var obj2 = new TestObj () { Id = "2", TestIntEnumerable = new List<int> { 4, 5, 6 } };

			var numIn1 = db.Insert (obj1);
			var numIn2 = db.Insert (obj2);
			Assert.AreEqual (1, numIn1);
			Assert.AreEqual (1, numIn2);

			var result = db.Query<TestObj> ("select * from TestObj").ToList ();
			Assert.AreEqual (2, result.Count);
			Assert.IsNotNull (result[0].TestIntEnumerable);
			Assert.IsNotNull (result[1].TestIntEnumerable);

			Assert.AreEqual(obj1.TestIntEnumerable.Count(), result[0].TestIntEnumerable.Count());
			Assert.AreEqual (obj2.TestIntEnumerable.Count (), result[1].TestIntEnumerable.Count ());

			for(var i = 0; i < obj1.TestIntEnumerable.Count(); i++) {
				Assert.AreEqual (obj1.TestIntEnumerable.ElementAt (i), result[0].TestIntEnumerable.ElementAt (i));
			}

			for (var i = 0; i < obj2.TestIntEnumerable.Count (); i++) {
				Assert.AreEqual (obj2.TestIntEnumerable.ElementAt (i), result[1].TestIntEnumerable.ElementAt (i));
			}

			Assert.AreEqual (obj1.Id, result[0].Id);
			Assert.AreEqual (obj2.Id, result[1].Id);

			db.Close ();
		}

		[Test]
		public void ShouldPersistAndReadEmptyIntEnumerables ()
		{
			var db = new TestDb (TestPath.GetTempFileName ());

			var obj1 = new TestObj () { Id = "1", TestIntEnumerable = Enumerable.Empty<int>() };
			var obj2 = new TestObj () { Id = "2" };

			var numIn1 = db.Insert (obj1);
			var numIn2 = db.Insert (obj2);
			Assert.AreEqual (1, numIn1);
			Assert.AreEqual (1, numIn2);

			var result = db.Query<TestObj> ("select * from TestObj").ToList ();
			Assert.AreEqual (2, result.Count);
			Assert.IsNotNull (result[0].TestIntEnumerable);
			Assert.AreEqual (0, result[0].TestIntEnumerable.Count ());

			Assert.IsNull (result[1].TestIntEnumerable);

			Assert.AreEqual (obj1.Id, result[0].Id);
			Assert.AreEqual (obj2.Id, result[1].Id);

			db.Close ();
		}

		[Test]
		public void ShouldPersistAndReadStringEnumerables ()
		{
			var db = new TestDb (TestPath.GetTempFileName ());

			var obj1 = new TestObj () { Id = "1", TestStringEnumerable = new List<string> { "1", "2", "3" } };
			var obj2 = new TestObj () { Id = "2", TestStringEnumerable = new List<string> { "String 4", "String 5", "String 6" } };

			var numIn1 = db.Insert (obj1);
			var numIn2 = db.Insert (obj2);
			Assert.AreEqual (1, numIn1);
			Assert.AreEqual (1, numIn2);

			var result = db.Query<TestObj> ("select * from TestObj").ToList ();
			Assert.AreEqual (2, result.Count);
			Assert.IsNotNull (result[0].TestStringEnumerable);
			Assert.IsNotNull (result[1].TestStringEnumerable);

			Assert.AreEqual (obj1.TestStringEnumerable.Count (), result[0].TestStringEnumerable.Count ());
			Assert.AreEqual (obj2.TestStringEnumerable.Count (), result[1].TestStringEnumerable.Count ());

			for (var i = 0; i < obj1.TestStringEnumerable.Count (); i++) {
				Assert.AreEqual (obj1.TestStringEnumerable.ElementAt (i), result[0].TestStringEnumerable.ElementAt (i));
			}

			for (var i = 0; i < obj2.TestStringEnumerable.Count (); i++) {
				Assert.AreEqual (obj2.TestStringEnumerable.ElementAt (i), result[1].TestStringEnumerable.ElementAt (i));
			}

			Assert.AreEqual (obj1.Id, result[0].Id);
			Assert.AreEqual (obj2.Id, result[1].Id);

			db.Close ();
		}

		[Test]
		public void ShouldPersistAndReadEmptyStringEnumerables ()
		{
			var db = new TestDb (TestPath.GetTempFileName ());

			var obj1 = new TestObj () { Id = "1", TestStringEnumerable = Enumerable.Empty<string> () };
			var obj2 = new TestObj () { Id = "2" };

			var numIn1 = db.Insert (obj1);
			var numIn2 = db.Insert (obj2);
			Assert.AreEqual (1, numIn1);
			Assert.AreEqual (1, numIn2);

			var result = db.Query<TestObj> ("select * from TestObj").ToList ();
			Assert.AreEqual (2, result.Count);
			Assert.IsNotNull (result[0].TestStringEnumerable);
			Assert.AreEqual (0, result[0].TestStringEnumerable.Count ());

			Assert.IsNull (result[1].TestStringEnumerable);

			Assert.AreEqual (obj1.Id, result[0].Id);
			Assert.AreEqual (obj2.Id, result[1].Id);

			db.Close ();
		}

		[Test]
		public void ShouldPersistAndReadObjectEnumerables ()
		{
			var db = new TestDb (TestPath.GetTempFileName ());

			var obj1 = new TestObj () {
				Id = "1",
				TestObjectEnumerable = new List<TestChildObject>
				{
					new TestChildObject { TestChildInt = 1, TestChildDate = DateTime.Now.AddSeconds(1), TestChildString = "String 1.1" },
					new TestChildObject { TestChildInt = 2, TestChildDate = DateTime.Now.AddSeconds(2), TestChildString = "String 1.2" },
					new TestChildObject { TestChildInt = 3, TestChildDate = DateTime.Now.AddSeconds(3), TestChildString = "String 1.3" },
				}
			};

			var obj2 = new TestObj () {
				Id = "2",
				TestObjectEnumerable = new List<TestChildObject>
				{
					new TestChildObject { TestChildInt = 4, TestChildDate = DateTime.Now.AddSeconds(4), TestChildString = "String 2.1" },
					new TestChildObject { TestChildInt = 5, TestChildDate = DateTime.Now.AddSeconds(5), TestChildString = "String 2.2" },
					new TestChildObject { TestChildInt = 6, TestChildDate = DateTime.Now.AddSeconds(6), TestChildString = "String 2.3" },
				}
			};

			var numIn1 = db.Insert (obj1);
			var numIn2 = db.Insert (obj2);
			Assert.AreEqual (1, numIn1);
			Assert.AreEqual (1, numIn2);

			var result = db.Query<TestObj> ("select * from TestObj").ToList ();
			Assert.AreEqual (2, result.Count);
			Assert.IsNotNull (result[0].TestObjectEnumerable);
			Assert.IsNotNull (result[1].TestObjectEnumerable);

			Assert.AreEqual (obj1.TestObjectEnumerable.Count (), result[0].TestObjectEnumerable.Count ());
			Assert.AreEqual (obj2.TestObjectEnumerable.Count (), result[1].TestObjectEnumerable.Count ());

			for (var i = 0; i < obj1.TestObjectEnumerable.Count (); i++) {
				Assert.AreEqual (obj1.TestObjectEnumerable.ElementAt (i), result[0].TestObjectEnumerable.ElementAt (i));
			}

			for (var i = 0; i < obj2.TestObjectEnumerable.Count (); i++) {
				Assert.AreEqual (obj2.TestObjectEnumerable.ElementAt (i), result[1].TestObjectEnumerable.ElementAt (i));
			}

			Assert.AreEqual (obj1.Id, result[0].Id);
			Assert.AreEqual (obj2.Id, result[1].Id);

			db.Close ();
		}

		[Test]
		public void ShouldPersistAndReadEmptyObjectEnumerables ()
		{
			var db = new TestDb (TestPath.GetTempFileName ());

			var obj1 = new TestObj () { Id = "1", TestObjectEnumerable = Enumerable.Empty<TestChildObject> () };
			var obj2 = new TestObj () { Id = "2" };

			var numIn1 = db.Insert (obj1);
			var numIn2 = db.Insert (obj2);
			Assert.AreEqual (1, numIn1);
			Assert.AreEqual (1, numIn2);

			var result = db.Query<TestObj> ("select * from TestObj").ToList ();
			Assert.AreEqual (2, result.Count);
			Assert.IsNotNull (result[0].TestObjectEnumerable);
			Assert.AreEqual (0, result[0].TestObjectEnumerable.Count ());

			Assert.IsNull (result[1].TestObjectEnumerable);

			Assert.AreEqual (obj1.Id, result[0].Id);
			Assert.AreEqual (obj2.Id, result[1].Id);

			db.Close ();
		}

	}
}
