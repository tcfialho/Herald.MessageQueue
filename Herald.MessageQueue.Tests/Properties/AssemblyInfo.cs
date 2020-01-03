using Xunit;
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
[assembly: TestCaseOrderer("Herald.MessageQueue.Tests.PriorityOrderer", "Herald.MessageQueue.Tests")]