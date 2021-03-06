﻿using System;
using System.Collections.Generic;
using System.Linq;
using ElasticsearchInside.Config;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.Search;
using Examine.Search;
using Nest;
using Novicell.Examine.ElasticSearch.Model;
using Novicell.Examine.ElasticSearch.Queries;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Novicell.Examine.ElasticSearch.Tests.Search
{
    [TestFixture]
    public class FluentApiTests
    {
        [Test]
        public void Managed_Range_Date()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("created", "datetime"))))
                {
                    indexer.CreateIndex();

                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(123.ToString(), "content",
                            new
                            {
                                created = new DateTime(2000, 01, 02),
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Home"
                            }),
                        ValueSet.FromObject(2123.ToString(), "content",
                            new
                            {
                                created = new DateTime(2000, 01, 04),
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Test"
                            }),
                        ValueSet.FromObject(3123.ToString(), "content",
                            new
                            {
                                created = new DateTime(2000, 01, 05),
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Page"
                            })
                    });


                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var numberSortedCriteria = searcher.CreateQuery()
                        .RangeQuery<DateTime>(new[] {"created"}, new DateTime(2000, 01, 02), new DateTime(2000, 01, 05),
                            maxInclusive: false);

                    var numberSortedResult = numberSortedCriteria.Execute();

                    Assert.AreEqual(2, numberSortedResult.TotalItemCount);
                }
            }
        }

        [Test]
        public void Managed_Full_Text()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer1 = new TestBaseIndex(config))
                {
                    indexer1.CreateIndex();
                    indexer1.IndexItem(ValueSet.FromObject("1", "content",
                        new
                        {
                            item1 = "value1",
                            item2 =
                                "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the total absolute darkness."
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("2", "content",
                        new
                        {
                            item1 = "value2",
                            item2 =
                                "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance."
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("3", "content",
                        new
                        {
                            item1 = "value3",
                            item2 =
                                "They are expected to confront the darkness and show evidence that they have done so in their papers"
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("4", "content",
                        new
                        {
                            item1 = "value4",
                            item2 =
                                "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness."
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("5", "content",
                        new {item1 = "value3", item2 = "Scotch scotch scotch, i love scotch"}));
                    indexer1.IndexItem(ValueSet.FromObject("6", "content",
                        new {item1 = "value4", item2 = "60% of the time, it works everytime"}));
                    indexer1._client.Value.Indices.Refresh(Indices.Index(indexer1.indexAlias));
                    var searcher = indexer1.GetSearcher();

                    var result = searcher.Search("darkness");

                    Assert.AreEqual(4, result.TotalItemCount);
                    Console.WriteLine("Search 1:");
                    foreach (var r in result)
                    {
                        Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                    }

                    result = searcher.Search("total darkness");
                    Assert.AreEqual(2, result.TotalItemCount);
                    Console.WriteLine("Search 2:");
                    foreach (var r in result)
                    {
                        Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                    }
                }
            }
        }

        [Test]
        public void Managed_Full_Text_With_Bool()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer1 = new TestBaseIndex(config))
                {
                    indexer1.CreateIndex();
                    indexer1.IndexItem(ValueSet.FromObject("1", "content",
                        new
                        {
                            item1 = "value1",
                            item2 =
                                "The agitated zebras gallop back and forth in short, panicky dashes, then skitter off into the total absolute darkness."
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("2", "content",
                        new
                        {
                            item1 = "value2",
                            item2 =
                                "The festival lasts five days and celebrates the victory of good over evil, light over darkness, and knowledge over ignorance."
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("3", "content",
                        new
                        {
                            item1 = "value3",
                            item2 =
                                "They are expected to confront the darkness and show evidence that they have done so in their papers"
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("4", "content",
                        new
                        {
                            item1 = "value4",
                            item2 =
                                "Scientists believe the lake could be home to cold-loving microbial life adapted to living in total darkness."
                        }));
                    indexer1.IndexItem(ValueSet.FromObject("5", "content",
                        new {item1 = "value3", item2 = "Scotch scotch scotch, i love scotch"}));
                    indexer1.IndexItem(ValueSet.FromObject("6", "content",
                        new {item1 = "value4", item2 = "60% of the time, it works everytime"}));
                    indexer1._client.Value.Indices.Refresh(Indices.Index(indexer1.indexAlias));
                    var searcher = indexer1.GetSearcher();

                    var qry = searcher.CreateQuery().ManagedQuery("darkness").And().Field("item1", "value1");
                    Console.WriteLine(qry);
                    var result = qry.Execute();

                    Assert.AreEqual(1, result.TotalItemCount);
                    Console.WriteLine("Search 1:");
                    foreach (var r in result)
                    {
                        Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                    }

                    qry = searcher.CreateQuery().ManagedQuery("darkness")
                        .And(query => query.Field("item1", "value1").Or().Field("item1", "value2"),
                            BooleanOperation.Or);
                    Console.WriteLine(qry);
                    result = qry.Execute();

                    Assert.AreEqual(2, result.TotalItemCount);
                    Console.WriteLine("Search 2:");
                    foreach (var r in result)
                    {
                        Console.WriteLine($"Id = {r.Id}, Score = {r.Score}");
                    }
                }
            }
        }

        [
            Test]
        public void Managed_Range_Int()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("parentID", "number"))
                ))
                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(123.ToString(), "content",
                            new
                            {
                                parentID = 121,
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Home"
                            }),
                        ValueSet.FromObject(2.ToString(), "content",
                            new
                            {
                                parentID = 123,
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Test"
                            }),
                        ValueSet.FromObject(3.ToString(), "content",
                            new
                            {
                                parentID = 124,
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Page"
                            })
                    });

                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var numberSortedCriteria = searcher.CreateQuery()
                        .RangeQuery<int>(new[] {"parentID"}, 122, 124);

                    var numberSortedResult = numberSortedCriteria.Execute();

                    Assert.AreEqual(2, numberSortedResult.TotalItemCount);
                }
            }
        }

        [Test]
        public void Legacy_ParentId()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("parentID", "number"))
                ))
                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(123.ToString(), "content",
                            new
                            {
                                nodeName = "my name 1",
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Home"
                            }),
                        ValueSet.FromObject(2.ToString(), "content",
                            new
                            {
                                parentID = 123,
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Test"
                            }),
                        ValueSet.FromObject(3.ToString(), "content",
                            new
                            {
                                parentID = 123,
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Page"
                            })
                    });

                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var numberSortedCriteria = searcher.CreateQuery()
                        .Field("parentID", 123)
                        .OrderBy(new SortableField("sortOrder", SortType.Int));

                    var numberSortedResult = numberSortedCriteria.Execute();
                    elasticsearch.Dispose();
                    Assert.AreEqual(2, numberSortedResult.TotalItemCount);
                }
            }
        }

        [Test]
        public void Grouped_Or_Examiness()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                        new FieldDefinitionCollection(
                            new FieldDefinition("nodeName", "text"),
                            new FieldDefinition("bodyText", "text"),
                            new FieldDefinition("nodeTypeAlias", "text")
                        )
                    )
                )
                {
                    indexer.CreateIndex();

                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new
                            {
                                nodeName = "my name 1",
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Home"
                            }),
                        ValueSet.FromObject(2.ToString(), "content",
                            new
                            {
                                nodeName = "About us",
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Test"
                            }),
                        ValueSet.FromObject(3.ToString(), "content",
                            new
                            {
                                nodeName = "my name 3",
                                bodyText = "lorem ipsum",
                                nodeTypeAlias = "CWS_Page"
                            })
                    });

                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //paths contain punctuation, we'll escape it and ensure an exact match
                    var criteria = searcher.CreateQuery("content");

                    //get all node type aliases starting with CWS_Home OR and all nodees starting with "About"
                    var filter = criteria.GroupedOr(
                        new[] {"nodeTypeAlias", "nodeName"},
                        new[] {"CWS\\_Home".Boost(10), "About".MultipleCharacterWildcard()});

                    var results = filter.Execute();
                    elasticsearch.Dispose();
                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Grouped_Or_Query_Output()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))

                {
                    var searcher = indexer.GetSearcher();

                    Console.WriteLine("GROUPED OR - SINGLE FIELD, MULTI VAL");
                    var criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedOr(new[] {"id"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED OR - MULTI FIELD, MULTI VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedOr(new[] {"id", "parentID"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual(
                        "+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3 parentID:1 parentID:2 parentID:3)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED OR - MULTI FIELD, EQUAL MULTI VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedOr(new[] {"id", "parentID", "blahID"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual(
                        "+__NodeTypeAlias:mydocumenttypealias +(id:1 id:2 id:3 parentID:1 parentID:2 parentID:3 blahID:1 blahID:2 blahID:3)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED OR - MULTI FIELD, SINGLE VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedOr(new[] {"id", "parentID"}.ToList(), new[] {"1"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1 parentID:1)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED OR - SINGLE FIELD, SINGLE VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedOr(new[] {"id"}.ToList(), new[] {"1"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(id:1)", criteria.Query.ToString());
                }
            }
        }

        /// <summary>
        /// Grouped AND is a special case as well since NOT and OR include all values, it doesn't make
        /// logic sense that AND includes all fields and values because nothing would actually match. 
        /// i.e. +id:1 +id2    --> Nothing matches
        /// </summary>
        [Test]
        public void Grouped_And_Query_Output()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))
                {
                    var searcher = indexer.GetSearcher();
                    //new LuceneSearcher("testSearcher", luceneDir, analyzer);

                    Console.WriteLine("GROUPED AND - SINGLE FIELD, MULTI VAL");
                    var criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedAnd(new[] {"id"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1)", criteria.Query.ToString());

                    Console.WriteLine("GROUPED AND - MULTI FIELD, EQUAL MULTI VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedAnd(new[] {"id", "parentID", "blahID"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:2 +blahID:3)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED AND - MULTI FIELD, MULTI VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedAnd(new[] {"id", "parentID"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:2)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED AND - MULTI FIELD, SINGLE VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedAnd(new[] {"id", "parentID"}.ToList(), new[] {"1"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1 +parentID:1)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED AND - SINGLE FIELD, SINGLE VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedAnd(new[] {"id"}.ToList(), new[] {"1"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias +(+id:1)", criteria.Query.ToString());
                }
            }
        }

        /// <summary>
        /// CANNOT BE A MUST WITH NOT i.e. +(-id:1 -id:2 -id:3)  --> That will not work with the "+"
        /// </summary>
        [Test]
        public void Grouped_Not_Query_Output()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))
                {
                    var searcher = indexer.GetSearcher();

                    Console.WriteLine("GROUPED NOT - SINGLE FIELD, MULTI VAL");
                    var criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedNot(new[] {"id"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias (-id:1 -id:2 -id:3)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED NOT - MULTI FIELD, MULTI VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedNot(new[] {"id", "parentID"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual(
                        "+__NodeTypeAlias:mydocumenttypealias (-id:1 -id:2 -id:3 -parentID:1 -parentID:2 -parentID:3)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED NOT - MULTI FIELD, EQUAL MULTI VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedNot(new[] {"id", "parentID", "blahID"}.ToList(), new[] {"1", "2", "3"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual(
                        "+__NodeTypeAlias:mydocumenttypealias (-id:1 -id:2 -id:3 -parentID:1 -parentID:2 -parentID:3 -blahID:1 -blahID:2 -blahID:3)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED NOT - MULTI FIELD, SINGLE VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedNot(new[] {"id", "parentID"}.ToList(), new[] {"1"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias (-id:1 -parentID:1)",
                        criteria.Query.ToString());

                    Console.WriteLine("GROUPED NOT - SINGLE FIELD, SINGLE VAL");
                    criteria = (ElasticSearchQuery) searcher.CreateQuery();
                    criteria.Field("__NodeTypeAlias", "myDocumentTypeAlias");
                    criteria.GroupedNot(new[] {"id"}.ToList(), new[] {"1"});
                    Console.WriteLine(criteria.Query);
                    Assert.AreEqual("+__NodeTypeAlias:mydocumenttypealias (-id:1)", criteria.Query.ToString());
                }
            }
        }

        [Test]
        public void Grouped_Or_With_Not()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))
                {
                    //TODO: Making this a number makes the query fail - i wonder how to make it work correctly?
                    // It's because the searching is NOT using a managed search
                    //new[] { new FieldDefinition("umbracoNaviHide", "number") }, 

                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new
                            {
                                nodeName = "my name 1", bodyText = "lorem ipsum", headerText = "header 1",
                                umbracoNaviHide = "1"
                            }),
                        ValueSet.FromObject(2.ToString(), "content",
                            new
                            {
                                nodeName = "my name 2", bodyText = "lorem ipsum", headerText = "header 2",
                                umbracoNaviHide = "0"
                            })
                    });

                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //paths contain punctuation, we'll escape it and ensure an exact match
                    var criteria = searcher.CreateQuery("content");
                    var filter = criteria.GroupedOr(new[] {"nodeName", "bodyText", "headerText"}, "ipsum").Not()
                        .Field("umbracoNaviHide", "1");
                    var results = filter.Execute();
                    elasticsearch.Dispose();
                    Assert.AreEqual(1, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Match_By_Path()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(
                    settings => settings
                        .EnableLogging()
                        .SetPort(9200)
                        .SetElasticsearchStartTimeout(180))
                .ReadySync()
            )
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("__Path", "raw"))
                ))
                {
                    indexer.CreateIndex();

                    indexer.IndexItems(new[]
                    {
                        new ValueSet(1.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 1"},
                                {"bodyText", "lorem ipsum"},
                                {"__Path", "-1,123,456,789"}
                            }),
                        new ValueSet(2.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 2"},
                                {"bodyText", "lorem ipsum"},
                                {"__Path", "-1,123,456,987"}
                            })
                    });


                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = (ElasticSearchSearcher) indexer.GetSearcher();

                    //paths contain punctuation, we'll escape it and ensure an exact match
                    var criteria = searcher.CreateQuery("content");
                    var filter = criteria.Field("__Path", "-1,123,456,789");
                    var results1 = filter.Execute();
                    Assert.AreEqual(0, results1.TotalItemCount);

                    //now escape it
                    var exactcriteria = searcher.CreateQuery("content");
                    var exactfilter = exactcriteria.Field("__Path", "-1,123,456,789".Escape());
                    var results2 = exactfilter.Execute();
                    Assert.AreEqual(1, results2.TotalItemCount);

                    //now try wildcards
                    var wildcardcriteria = searcher.CreateQuery("content");
                    var wildcardfilter = wildcardcriteria.Field("__Path", "-1,123,456,".MultipleCharacterWildcard());
                    var results3 = wildcardfilter.Execute();
                    Assert.AreEqual(2, results3.TotalItemCount);
                    //not found
                    wildcardcriteria = searcher.CreateQuery("content");
                    wildcardfilter = wildcardcriteria.Field("__Path", "-1,123,457,".MultipleCharacterWildcard());
                    results3 = wildcardfilter.Execute();
                    Assert.AreEqual(0, results3.TotalItemCount);
                }
            }
        }

        [Test]
        public void Find_By_ParentId()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("parentID", "number"))))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "my name 1", bodyText = "lorem ipsum", parentID = "1235"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "my name 2", bodyText = "lorem ipsum", parentID = "1139"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "my name 3", bodyText = "lorem ipsum", parentID = "1139"})
                    });


                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var criteria = searcher.CreateQuery("content");
                    var filter = criteria.Field("parentID", 1139);

                    var results = filter.Execute();

                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Find_By_NodeTypeAlias()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("nodeTypeAlias", "raw"))))
                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        new ValueSet(1.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 1"},
                                {"nodeTypeAlias", "CWS_Home"}
                                //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                            }),
                        new ValueSet(2.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 2"},
                                {"nodeTypeAlias", "CWS_Home"}
                                //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                            }),
                        new ValueSet(3.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 3"},
                                {"nodeTypeAlias", "CWS_Page"}
                                //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Page"}
                            })
                    });


                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var criteria = searcher.CreateQuery("content");
                    var filter = criteria.Field("nodeTypeAlias", "CWS_Home".Escape());

                    var results = filter.Execute();


                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        /* TODO: Figure out if we need that test
        [Test]
        public void Search_With_Stop_Words()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()

                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,null, "StopAnalyzer"))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new
                            {
                                nodeName = "into 1",
                                bodyText = "It was one thing to bring Carmen into it, but Jonathan was another story."
                            }),
                        ValueSet.FromObject(2.ToString(), "content",
                            new
                            {
                                nodeName = "my name 2",
                                bodyText =
                                    "Hands shoved backwards into his back pockets, he took slow deliberate steps, as if he had something on his mind."
                            }),
                        ValueSet.FromObject(3.ToString(), "content",
                            new
                            {
                                nodeName = "my name 3",
                                bodyText = "Slowly carrying the full cups into the living room, she handed one to Alex."
                            })
                    });

                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var criteria = searcher.CreateQuery();
                    var filter = criteria.Field("bodyText", "into")
                        .Or().Field("nodeName", "into");

                    var results = filter.Execute();

                    Assert.AreEqual(0, results.TotalItemCount);
                }
            }
        }
*/
        [Test]
        public void Search_Raw_Query()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("nodeTypeAlias", "text"))))


                {
                    indexer.CreateIndex();

                    indexer.IndexItems(new[]
                    {
                        new ValueSet(1.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 1"},
                                {"nodeTypeAlias", "CWS_Home"}
                                //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                            }),
                        new ValueSet(2.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 2"},
                                {"nodeTypeAlias", "CWS_Home"}
                                //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Home"}
                            }),
                        new ValueSet(3.ToString(), "content",
                            new Dictionary<string, object>
                            {
                                {"nodeName", "my name 3"},
                                {"nodeTypeAlias", "CWS_Page"}
                                //{UmbracoContentIndexer.NodeTypeAliasFieldName, "CWS_Page"}
                            })
                    });

                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var criteria = searcher.CreateQuery("content");

                    var results = criteria.NativeQuery("nodeTypeAlias:CWS_Home").Execute();

                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Find_Only_Image_Media()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(
                        new FieldDefinition("nodeName", "text"),
                        new FieldDefinition("bodyText", "text"),
                        new FieldDefinition("nodeTypeAlias", "text")
                    )
                ))

                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "media",
                            new {nodeName = "my name 1", bodyText = "lorem ipsum", nodeTypeAlias = "image"}),
                        ValueSet.FromObject(2.ToString(), "media",
                            new {nodeName = "my name 2", bodyText = "lorem ipsum", nodeTypeAlias = "image"}),
                        ValueSet.FromObject(3.ToString(), "media",
                            new {nodeName = "my name 3", bodyText = "lorem ipsum", nodeTypeAlias = "file"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var criteria = searcher.CreateQuery("media");
                    var filter = criteria.Field("nodeTypeAlias", "image");

                    var results = filter.Execute();

                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Find_Both_Media_And_Content()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))


                {
                    indexer.CreateIndex();

                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "media",
                            new {nodeName = "my name 1", bodyText = "lorem ipsum", nodeTypeAlias = "image"}),
                        ValueSet.FromObject(2.ToString(), "media",
                            new {nodeName = "my name 2", bodyText = "lorem ipsum", nodeTypeAlias = "image"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "my name 3", bodyText = "lorem ipsum", nodeTypeAlias = "file"}),
                        ValueSet.FromObject(4.ToString(), "other",
                            new {nodeName = "my name 4", bodyText = "lorem ipsum", nodeTypeAlias = "file"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var criteria = searcher.CreateQuery(defaultOperation: BooleanOperation.Or);
                    var filter = criteria
                        .Field(LuceneIndex.CategoryFieldName, "media")
                        .Or()
                        .Field(LuceneIndex.CategoryFieldName, "content");

                    var results = filter.Execute();

                    Assert.AreEqual(3, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Sort_Result_By_Number_Field()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    //Ensure it's set to a number, otherwise it's not sortable
                    new FieldDefinitionCollection(new FieldDefinition("sortOrder", "number"),
                        new FieldDefinition("parentID", "number"))))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "my name 1", sortOrder = "3", parentID = "1143"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "my name 2", sortOrder = "1", parentID = "1143"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "my name 3", sortOrder = "2", parentID = "1143"}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "my name 4", bodyText = "lorem ipsum", parentID = "2222"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var sc = searcher.CreateQuery("content");
                    var sc1 = sc.Field("parentID", 1143).OrderBy(new SortableField("sortOrder", SortType.Int));

                    var results1 = sc1.Execute().ToArray();

                    Assert.AreEqual(3, results1.Length);

                    var currSort = 0;
                    for (var i = 0; i < results1.Length; i++)
                    {
                        Assert.GreaterOrEqual(int.Parse(results1[i].Values["sortOrder"]), currSort);
                        currSort = int.Parse(results1[i].Values["sortOrder"]);
                    }
                }
            }
        }

        [Test]
        public void Sort_Result_By_Date_Field()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    //Ensure it's set to a date, otherwise it's not sortable
                    new FieldDefinitionCollection(new FieldDefinition("updateDate", "date"),
                        new FieldDefinition("parentID", "number"))))


                {
                    var now = DateTime.Now;
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new
                            {
                                nodeName = "my name 1", updateDate = now.AddDays(2).ToString("yyyy-MM-dd"),
                                parentID = "1143"
                            }),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "my name 2", updateDate = now.ToString("yyyy-MM-dd"), parentID = 1143}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new
                            {
                                nodeName = "my name 3", updateDate = now.AddDays(1).ToString("yyyy-MM-dd"),
                                parentID = 1143
                            }),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "my name 4", updateDate = now, parentID = "2222"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var sc = searcher.CreateQuery("content");
                    var sc1 = sc.Field("parentID", 1143).OrderBy(new SortableField("updateDate", SortType.Double));

                    var results1 = sc1.Execute().ToArray();

                    Assert.AreEqual(3, results1.Length);

                    double currSort = 0;
                    for (var i = 0; i < results1.Length; i++)
                    {
                        Assert.GreaterOrEqual(Convert.ToDateTime(results1[i].Values["updateDate"]).ToOADate(),
                            currSort);
                        currSort = Convert.ToDateTime(results1[i].Values["updateDate"]).ToOADate();
                    }
                }
            }
        }

        /* TODO: As elastic doesn't support sorting on text fields by default figure out if should be added multifields defintion on every text field
               [Test]
               public void Sort_Result_By_Single_Field()
               {
                   using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                       .EnableLogging()
       
                       .SetElasticsearchStartTimeout(180)).ReadySync())
                   {
                       ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                       using (var indexer = new TestBaseIndex(config,
                           //Ensure it's set to a fulltextsortable, otherwise it's not sortable
                           new FieldDefinitionCollection(new FieldDefinition("nodeName", "fulltextsortable"))))
       
       
                       {
                           indexer.CreateIndex();
                           indexer.IndexItems(new[]
                           {
                               ValueSet.FromObject(1.ToString(), "content",
                                   new {nodeName = "my name 1", writerName = "administrator", parentID = "1143"}),
                               ValueSet.FromObject(2.ToString(), "content",
                                   new {nodeName = "my name 2", writerName = "administrator", parentID = "1143"}),
                               ValueSet.FromObject(3.ToString(), "content",
                                   new {nodeName = "my name 3", writerName = "administrator", parentID = "1143"}),
                               ValueSet.FromObject(4.ToString(), "content",
                                   new {nodeName = "my name 4", writerName = "writer", parentID = "2222"})
                           });
       
                           indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                           var searcher = indexer.GetSearcher();
       
                           var sc = searcher.CreateQuery("content");
                           var sc1 = sc.Field("writerName", "administrator")
                               .OrderBy(new SortableField("nodeName", SortType.String));
       
                           sc = searcher.CreateQuery("content");
                           var sc2 = sc.Field("writerName", "administrator")
                               .OrderByDescending(new SortableField("nodeName", SortType.String));
       
                           var results1 = sc1.Execute();
                           var results2 = sc2.Execute();
       
                           Assert.AreNotEqual(results1.First().Id, results2.First().Id);
                       }
                   }
               }
       */
        [Test]
        public void Standard_Results_Sorted_By_Score()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))


                {
                    indexer.CreateIndex();


                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "umbraco", headerText = "world", bodyText = "blah"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "umbraco", headerText = "umbraco", bodyText = "blah"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "umbraco", headerText = "umbraco", bodyText = "umbraco"}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "hello", headerText = "world", bodyText = "blah"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    var sc = searcher.CreateQuery("content", BooleanOperation.Or);
                    var sc1 = sc.Field("nodeName", "umbraco").Or().Field("headerText", "umbraco").Or()
                        .Field("bodyText", "umbraco");

                    var results = sc1.Execute();

                    //Assert
                    for (int i = 0; i < results.TotalItemCount - 1; i++)
                    {
                        var curr = results.ElementAt(i);
                        var next = results.ElementAtOrDefault(i + 1);

                        if (next == null)
                            break;

                        Assert.IsTrue(curr.Score >= next.Score,
                            string.Format("Result at index {0} must have a higher score than result at index {1}", i,
                                i + 1));
                    }
                }
            }
        }

        [Test]
        public void Skip_Results_Returns_Different_Results()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "umbraco", headerText = "world", writerName = "administrator"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "umbraco", headerText = "umbraco", writerName = "administrator"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "umbraco", headerText = "umbraco", writerName = "administrator"}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "hello", headerText = "world", writerName = "blah"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //Arrange
                    var sc = searcher.CreateQuery("content").Field("writerName", "administrator");

                    //Act
                    var results = sc.Execute();

                    //Assert
                    Assert.AreNotEqual(results.First(), results.Skip(2).First(), "Third result should be different");
                }
            }
        }

        [Test]
        public void Escaping_Includes_All_Words()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "codegarden09", headerText = "world", writerName = "administrator"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "codegarden 09", headerText = "umbraco", writerName = "administrator"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "codegarden  09", headerText = "umbraco", writerName = "administrator"}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "codegarden 090", headerText = "world", writerName = "blah"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //Arrange
                    var sc = searcher.CreateQuery("content").Field("nodeName", "codegarden 09".Escape());

                    //Act
                    var results = sc.Execute();

                    //Assert
                    //NOTE: The result is 2 because the double space is removed with the analyzer
                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Grouped_And_Examiness()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "Aloha", nodeTypeAlias = "CWS_Hello"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "Helo", nodeTypeAlias = "CWS_World"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "Another node", nodeTypeAlias = "SomethingElse"}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "Always consider this", nodeTypeAlias = "CWS_World"})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //Arrange
                    var criteria = searcher.CreateQuery("content");

                    //get all node type aliases starting with CWS and all nodees starting with "A"
                    var filter = criteria.GroupedAnd(
                        new[] {"nodeTypeAlias", "nodeName"},
                        new[] {"CWS".MultipleCharacterWildcard(), "A".MultipleCharacterWildcard()});


                    //Act
                    var results = filter.Execute();

                    //Assert
                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        [Test]
        public void Examiness_Proximity()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "Aloha", metaKeywords = "Warren is likely to be creative"}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "Helo", metaKeywords = "Creative is Warren's middle name"}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new
                            {
                                nodeName = "Another node",
                                metaKeywords = "If Warren were creative... well, he actually is"
                            }),
                        ValueSet.FromObject(4.ToString(), "content",
                            new
                            {
                                nodeName = "Always consider this",
                                metaKeywords = "Warren is a very talented individual and quite creative"
                            })
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //Arrange
                    var criteria = searcher.CreateQuery("content");

                    //get all nodes that contain the words warren and creative within 5 words of each other
                    var filter = criteria.Field("metaKeywords", "Warren creative".Proximity(5));

                    //Act
                    var results = filter.Execute();

                    //Assert - 

                    Assert.AreEqual(2, results.TotalItemCount);
                }
            }
        }

        /// <summary>
        /// test range query with a Float structure
        /// </summary>
        [Test]
        public void Float_Range_SimpleIndexSet()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    //Ensure it's set to a float
                    new FieldDefinitionCollection(new FieldDefinition("SomeFloat", "float"))))


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "Aloha", SomeFloat = 1}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "Helo", SomeFloat = 123}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "Another node", SomeFloat = 12}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "Always consider this", SomeFloat = 25})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //all numbers should be between 0 and 100 based on the data source
                    var criteria1 = searcher.CreateQuery();
                    var filter1 = criteria1.RangeQuery<float>(new[] {"SomeFloat"}, 0f, 100f, true, true);

                    var criteria2 = searcher.CreateQuery();
                    var filter2 = criteria2.RangeQuery<float>(new[] {"SomeFloat"}, 101f, 200f, true, true);

                    //Act
                    var results1 = filter1.Execute();
                    var results2 = filter2.Execute();

                    //Assert
                    Assert.AreEqual(3, results1.TotalItemCount);
                    Assert.AreEqual(1, results2.TotalItemCount);
                }
            }
        }

        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [Test]
        public void Number_Range_SimpleIndexSet()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    //Ensure it's set to a float
                    new FieldDefinitionCollection(new FieldDefinition("SomeNumber", "number"))))


                {
                    indexer.CreateIndex();

                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "Aloha", SomeNumber = 1}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "Helo", SomeNumber = 123}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "Another node", SomeNumber = 12}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "Always consider this", SomeNumber = 25})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //all numbers should be between 0 and 100 based on the data source
                    var criteria1 = searcher.CreateQuery();
                    var filter1 = criteria1.RangeQuery<int>(new[] {"SomeNumber"}, 0, 100, true, true);

                    var criteria2 = searcher.CreateQuery();
                    var filter2 = criteria2.RangeQuery<int>(new[] {"SomeNumber"}, 101, 200, true, true);

                    //Act
                    var results1 = filter1.Execute();
                    var results2 = filter2.Execute();

                    //Assert
                    Assert.AreEqual(3, results1.TotalItemCount);
                    Assert.AreEqual(1, results2.TotalItemCount);
                }
            }
        }

        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [Test]
        public void Double_Range_SimpleIndexSet()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    //Ensure it's set to a float
                    new FieldDefinitionCollection(new FieldDefinition("SomeDouble", "double"))
                ))


                {
                    indexer.CreateIndex();

                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "Aloha", SomeDouble = 1d}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "Helo", SomeDouble = 123d}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "Another node", SomeDouble = 12d}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "Always consider this", SomeDouble = 25d})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //all numbers should be between 0 and 100 based on the data source
                    var criteria1 = searcher.CreateQuery();
                    var filter1 = criteria1.RangeQuery<double>(new[] {"SomeDouble"}, 0d, 100d, true, true);

                    var criteria2 = searcher.CreateQuery();
                    var filter2 = criteria2.RangeQuery<double>(new[] {"SomeDouble"}, 101d, 200d, true, true);

                    //Act
                    var results1 = filter1.Execute();
                    var results2 = filter2.Execute();

                    //Assert
                    Assert.AreEqual(3, results1.TotalItemCount);
                    Assert.AreEqual(1, results2.TotalItemCount);
                }
            }
        }

        /// <summary>
        /// test range query with a Double structure
        /// </summary>
        [Test]
        public void Long_Range_SimpleIndexSet()
        {
            using (var elasticsearch = new ElasticsearchInside.Elasticsearch(settings => settings
                .EnableLogging()
                .SetPort(9200)
                .SetElasticsearchStartTimeout(180)).ReadySync())
            {
                ElasticSearchConfig config = new ElasticSearchConfig(new ConnectionSettings(elasticsearch.Url));
                using (var indexer = new TestBaseIndex(config,
                    new FieldDefinitionCollection(new FieldDefinition("SomeLong", "long")))
                )


                {
                    indexer.CreateIndex();
                    indexer.IndexItems(new[]
                    {
                        ValueSet.FromObject(1.ToString(), "content",
                            new {nodeName = "Aloha", SomeLong = 1L}),
                        ValueSet.FromObject(2.ToString(), "content",
                            new {nodeName = "Helo", SomeLong = 123L}),
                        ValueSet.FromObject(3.ToString(), "content",
                            new {nodeName = "Another node", SomeLong = 12L}),
                        ValueSet.FromObject(4.ToString(), "content",
                            new {nodeName = "Always consider this", SomeLong = 25L})
                    });
                    indexer._client.Value.Indices.Refresh(Indices.Index(indexer.indexAlias));
                    var searcher = indexer.GetSearcher();

                    //all numbers should be between 0 and 100 based on the data source
                    var criteria1 = searcher.CreateQuery();
                    var filter1 = criteria1.RangeQuery<long>(new[] {"SomeLong"}, 0L, 100L, true, true);

                    var criteria2 = searcher.CreateQuery();
                    var filter2 = criteria2.RangeQuery<long>(new[] {"SomeLong"}, 101L, 200L, true, true);

                    //Act
                    var results1 = filter1.Execute();
                    var results2 = filter2.Execute();

                    //Assert
                    Assert.AreEqual(3, results1.TotalItemCount);
                    Assert.AreEqual(1, results2.TotalItemCount);
                }
            }
        }

        ///// <summary>
        ///// test range query with a Date.Minute structure
        ///// </summary>
        //[Test]
        //public void Date_Range_Minute_SimpleIndexSet()
        //{
        //    var reIndexDateTime = DateTime.Now.AddMinutes(-2);

        //    var analyzer = new StandardAnalyzer(Version.LUCENE_30);
        //    using (var luceneDir = new RandomIdRAMDirectory())
        //    using (var indexer = new TestIndexer(

        //        new[] { new FieldDefinition("MinuteCreated", "date.minute") },
        //        luceneDir, analyzer))


        //    {
        //        

        //        indexer.IndexItems(new[] {
        //            ValueSet.FromObject(1.ToString(), "content",
        //                new { MinuteCreated = reIndexDateTime }),
        //            ValueSet.FromObject(2.ToString(), "content",
        //                new { MinuteCreated = reIndexDateTime }),
        //            ValueSet.FromObject(3.ToString(), "content",
        //                new { MinuteCreated = reIndexDateTime.AddMinutes(-10) }),
        //            ValueSet.FromObject(4.ToString(), "content",
        //                new { MinuteCreated = reIndexDateTime })
        //            });


        //        var searcher = new LuceneSearcher("testSearcher", luceneDir, analyzer);

        //        var criteria = searcher.CreateCriteria();
        //        var filter = criteria.Range("MinuteCreated",
        //            reIndexDateTime, DateTime.Now, true, true, DateResolution.Minute).Compile();

        //        var criteria2 = searcher.CreateCriteria();
        //        var filter2 = criteria2.Range("MinuteCreated",
        //            reIndexDateTime.AddMinutes(-20), reIndexDateTime.AddMinutes(-1), true, true, DateResolution.Minute).Compile();

        //        ////Act
        //        var results = searcher.Search(filter);
        //        var results2 = searcher.Search(filter2);

        //        ////Assert
        //        Assert.AreEqual(3, results.TotalItemCount);
        //        Assert.AreEqual(1, results2.TotalItemCount);
        //    }

        //}

        ///// <summary>
        ///// test range query with a Date.Hour structure
        ///// </summary>
        //[Test]
        //public void Date_Range_Hour_SimpleIndexSet()
        //{
        //    var reIndexDateTime = DateTime.Now.AddHours(-2);

        //    var analyzer = new StandardAnalyzer(Version.LUCENE_30);
        //    using (var luceneDir = new RandomIdRAMDirectory())
        //    using (var indexer = new TestIndexer(

        //        new[] { new FieldDefinition("HourCreated", "date.hour") },
        //        luceneDir, analyzer))


        //    {

        //        

        //        indexer.IndexItems(new[] {
        //            ValueSet.FromObject(1.ToString(), "content",
        //                new { HourCreated = reIndexDateTime }),
        //            ValueSet.FromObject(2.ToString(), "content",
        //                new { HourCreated = reIndexDateTime }),
        //            ValueSet.FromObject(3.ToString(), "content",
        //                new { HourCreated = reIndexDateTime.AddHours(-10) }),
        //            ValueSet.FromObject(4.ToString(), "content",
        //                new { HourCreated = reIndexDateTime })
        //            });


        //        var searcher = new LuceneSearcher("testSearcher", luceneDir, analyzer);

        //        var criteria = searcher.CreateCriteria();
        //        var filter = criteria.Range("HourCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Hour).Compile();

        //        var criteria2 = searcher.CreateCriteria();
        //        var filter2 = criteria2.Range("HourCreated", reIndexDateTime.AddHours(-20), reIndexDateTime.AddHours(-3), true, true, DateResolution.Hour).Compile();

        //        ////Act
        //        var results = searcher.Search(filter);
        //        var results2 = searcher.Search(filter2);

        //        ////Assert
        //        Assert.AreEqual(3, results.TotalItemCount);
        //        Assert.AreEqual(1, results2.TotalItemCount);
        //    }
        //}

        ///// <summary>
        ///// test range query with a Date.Day structure
        ///// </summary>
        //[Test]
        //public void Date_Range_Day_SimpleIndexSet()
        //{
        //    var reIndexDateTime = DateTime.Now.AddDays(-2);

        //    var analyzer = new StandardAnalyzer(Version.LUCENE_30);
        //    using (var luceneDir = new RandomIdRAMDirectory())
        //    using (var indexer = new TestIndexer(

        //        new[] { new FieldDefinition("DayCreated", "date.day") },
        //        luceneDir, analyzer))


        //    {
        //        

        //        indexer.IndexItems(new[] {
        //            ValueSet.FromObject(1.ToString(), "content",
        //                new { DayCreated = reIndexDateTime }),
        //            ValueSet.FromObject(2.ToString(), "content",
        //                new { DayCreated = reIndexDateTime }),
        //            ValueSet.FromObject(3.ToString(), "content",
        //                new { DayCreated = reIndexDateTime.AddDays(-10) }),
        //            ValueSet.FromObject(4.ToString(), "content",
        //                new { DayCreated = reIndexDateTime })
        //            });


        //        var searcher = new LuceneSearcher("testSearcher", luceneDir, analyzer);

        //        var criteria = searcher.CreateCriteria();
        //        var filter = criteria.Range("DayCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Day).Compile();

        //        var criteria2 = searcher.CreateCriteria();
        //        var filter2 = criteria2.Range("DayCreated", reIndexDateTime.AddDays(-20), reIndexDateTime.AddDays(-3), true, true, DateResolution.Day).Compile();

        //        ////Act
        //        var results = searcher.Search(filter);
        //        var results2 = searcher.Search(filter2);

        //        ////Assert
        //        Assert.AreEqual(3, results.TotalItemCount);
        //        Assert.AreEqual(1, results2.TotalItemCount);
        //    }

        //}

        ///// <summary>
        ///// test range query with a Date.Month structure
        ///// </summary>
        //[Test]
        //public void Date_Range_Month_SimpleIndexSet()
        //{
        //    var reIndexDateTime = DateTime.Now.AddMonths(-2);

        //    var analyzer = new StandardAnalyzer(Version.LUCENE_30);
        //    using (var luceneDir = new RandomIdRAMDirectory())
        //    using (var indexer = new TestIndexer(

        //        new[] { new FieldDefinition("MonthCreated", "date.month") },
        //        luceneDir, analyzer))


        //    {
        //        

        //        indexer.IndexItems(new[] {
        //            ValueSet.FromObject(1.ToString(), "content",
        //                new { MonthCreated = reIndexDateTime }),
        //            ValueSet.FromObject(2.ToString(), "content",
        //                new { MonthCreated = reIndexDateTime }),
        //            ValueSet.FromObject(3.ToString(), "content",
        //                new { MonthCreated = reIndexDateTime.AddMonths(-10) }),
        //            ValueSet.FromObject(4.ToString(), "content",
        //                new { MonthCreated = reIndexDateTime })
        //            });


        //        var searcher = new LuceneSearcher("testSearcher", luceneDir, analyzer);

        //        var criteria = searcher.CreateCriteria();
        //        var filter = criteria.Range("MonthCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Month).Compile();

        //        var criteria2 = searcher.CreateCriteria();
        //        var filter2 = criteria2.Range("MonthCreated", reIndexDateTime.AddMonths(-20), reIndexDateTime.AddMonths(-3), true, true, DateResolution.Month).Compile();

        //        ////Act
        //        var results = searcher.Search(filter);
        //        var results2 = searcher.Search(filter2);

        //        ////Assert
        //        Assert.AreEqual(3, results.TotalItemCount);
        //        Assert.AreEqual(1, results2.TotalItemCount);
        //    }


        //}

        ///// <summary>
        ///// test range query with a Date.Year structure
        ///// </summary>
        //[Test]
        //public void Date_Range_Year_SimpleIndexSet()
        //{
        //    var reIndexDateTime = DateTime.Now.AddYears(-2);

        //    var analyzer = new StandardAnalyzer(Version.LUCENE_30);
        //    using (var luceneDir = new RandomIdRAMDirectory())
        //    using (var indexer = new TestIndexer(

        //        new[] { new FieldDefinition("YearCreated", "date.year") },
        //        luceneDir, analyzer))


        //    {
        //        

        //        indexer.IndexItems(new[] {
        //            ValueSet.FromObject(1.ToString(), "content",
        //                new { YearCreated = reIndexDateTime }),
        //            ValueSet.FromObject(2.ToString(), "content",
        //                new { YearCreated = reIndexDateTime }),
        //            ValueSet.FromObject(3.ToString(), "content",
        //                new { YearCreated = reIndexDateTime.AddMonths(-10) }),
        //            ValueSet.FromObject(4.ToString(), "content",
        //                new { YearCreated = reIndexDateTime })
        //            });


        //        var searcher = new LuceneSearcher("testSearcher", luceneDir, analyzer);

        //        var criteria = searcher.CreateCriteria();

        //        var filter = criteria.Range("YearCreated", reIndexDateTime, DateTime.Now, true, true, DateResolution.Year).Compile();

        //        var criteria2 = searcher.CreateCriteria();
        //        var filter2 = criteria2.Range("YearCreated", DateTime.Now.AddYears(-20), DateTime.Now.AddYears(-3), true, true, DateResolution.Year).Compile();

        //        ////Act
        //        var results = searcher.Search(filter);
        //        var results2 = searcher.Search(filter2);

        //        ////Assert
        //        Assert.AreEqual(3, results.TotalItemCount);
        //        Assert.AreEqual(1, results2.TotalItemCount);
        //    }
        //}
/*
 TODO: Migrate rest of test
        /// <summary>
        /// Test range query with a DateTime structure
        /// </summary>
        [Test]
        public void Date_Range_SimpleIndexSet()
        {
            var reIndexDateTime = DateTime.Now;

            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestBaseIndex(

                new FieldDefinitionCollection(new FieldDefinition("DateCreated", "datetime"))))
            

            {
                

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { DateCreated = reIndexDateTime }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { DateCreated = reIndexDateTime }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { DateCreated = reIndexDateTime.AddMonths(-10) }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { DateCreated = reIndexDateTime })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();
                var filter = criteria.RangeQuery<DateTime>(new []{ "DateCreated" }, reIndexDateTime, DateTime.Now, true, true);

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.RangeQuery<DateTime>(new[] { "DateCreated" }, reIndexDateTime.AddDays(-1), reIndexDateTime.AddSeconds(-1), true, true);

                ////Act
                var results = filter.Execute();
                var results2 = filter2.Execute();

                ////Assert
                Assert.IsTrue(results.TotalItemCount > 0);
                Assert.IsTrue(results2.TotalItemCount == 0);
            }


        }

        [Test]
        public void Fuzzy_Search()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestBaseIndex(luceneDir, analyzer))
            

            {
                

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "I'm thinking here" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "I'm a thinker" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "I am pretty thoughtful" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "I thought you were cool" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();
                var filter = criteria.Field("Content", "think".Fuzzy(0.1F));

                var criteria2 = searcher.CreateQuery();
                var filter2 = criteria2.Field("Content", "thought".Fuzzy());

                ////Act
                var results = filter.Execute();
                var results2 = filter2.Execute();

                ////Assert
                Assert.AreEqual(2, results.TotalItemCount);
                Assert.AreEqual(2, results2.TotalItemCount);

            }


        }


        [Test]
        public void Max_Count()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestBaseIndex(luceneDir, analyzer))


            {
                

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello worlds" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you cruel world" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hi there, hello world" })
                });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();
                var filter = criteria.Field("Content", "hello");

                //Act
                var results = filter.Execute(3);

                //Assert

                Assert.AreEqual(3, results.Count());

                //NOTE: These are the total matched! The actual results are limited
                Assert.AreEqual(4, results.TotalItemCount);

            }

        }


        [Test]
        public void Inner_Or_Query()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestBaseIndex(luceneDir, analyzer))
            

            {
                

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();

                //Query = 
                //  +Type:type1 +(Content:world Content:something)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").Or().Field("Content", "something"), BooleanOperation.Or);

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Inner_And_Query()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestBaseIndex(luceneDir, analyzer))
            

            {
                

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or world", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();

                //Query = 
                //  +Type:type1 +(+Content:world +Content:hello)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").And().Field("Content", "hello"));

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }

        [Test]
        public void Inner_Not_Query()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestBaseIndex(luceneDir, analyzer))
            

            {
                

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or world", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery();

                //Query = 
                //  +Type:type1 +(+Content:world -Content:something)

                var filter = criteria.Field("Type", "type1")
                    .And(query => query.Field("Content", "world").Not().Field("Content", "something"));

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(1, results.TotalItemCount);
            }
        }

        [Test]
        public void Complex_Or_Group_Nested_Query()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestBaseIndex(luceneDir, analyzer))
            

            {
                

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { Content = "hello world", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello something or other", Type = "type1" }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { Content = "hello you guys", Type = "type1" }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { Content = "hello you cruel world", Type = "type2" }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { Content = "hi there, hello world", Type = "type2" })
                    });

                var searcher = indexer.GetSearcher();

                var criteria = searcher.CreateQuery(defaultOperation: BooleanOperation.Or);

                //Query = 
                //  (Type:type1 +(Content:world Content:something)) (Type:type2 +(+Content:world +Content:cruel))

                var filter = criteria
                    .Group(group => group.Field("Type", "type1")
                        .And(query => query.Field("Content", "world").Or().Field("Content", "something"), BooleanOperation.Or))
                    .Or()
                    .Group(group => group.Field("Type", "type2")
                        .And(query => query.Field("Content", "world").And().Field("Content", "cruel")));

                //Act
                var results = filter.Execute();

                //Assert
                Assert.AreEqual(3, results.TotalItemCount);
            }
        }


        //[Test]
        //public void Custom_Lucene_Query_With_Raw()
        //{
        //    var criteria = (LuceneSearchCriteria)_searcher.CreateCriteria("content");

        //    //combine a custom lucene query with raw lucene query
        //    criteria = (LuceneSearchCriteria)criteria.RawQuery("hello:world");
        //    criteria.LuceneQuery(NumericRangeQuery.NewLongRange("numTest", 4, 5, true, true));

        //  Console.WriteLine(criteria.Query);
        //    Assert.AreEqual("+hello:world +numTest:[4 TO 5]", criteria.Query.ToString());
        //}



        //[Test]
        //public void Wildcard_Results_Sorted_By_Score()
        //{
        //    //Arrange
        //    var sc = _searcher.CreateCriteria("content", SearchCriteria.BooleanOperation.Or);

        //    //set the rewrite method before adding queries
        //    var lsc = (LuceneSearchCriteria)sc;
        //    lsc.QueryParser.MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE;

        //    sc = sc.NodeName("umbrac".MultipleCharacterWildcard())
        //        .Or().Field("headerText", "umbrac".MultipleCharacterWildcard())
        //        .Or().Field("bodyText", "umbrac".MultipleCharacterWildcard()).Compile();

        //    //Act
        //    var results = _searcher.Search(sc);

        //    Assert.Greater(results.TotalItemCount, 0);

        //    //Assert
        //    for (int i = 0; i < results.TotalItemCount - 1; i++)
        //    {
        //        var curr = results.ElementAt(i);
        //        var next = results.ElementAtOrDefault(i + 1);

        //        if (next == null)
        //            break;

        //        Assert.IsTrue(curr.Score > next.Score, $"Result at index {i} must have a higher score than result at index {i + 1}");
        //    }
        //}

        //[Test]
        //public void Wildcard_Results_Sorted_By_Score_TooManyClauses_Exception()
        //{
        //    //this will throw during rewriting because 'lo*' matches too many things but with the work around in place this shouldn't throw
        //    // but it will use a constant score rewrite
        //    BooleanQuery.MaxClauseCount =3;

        //    try
        //    {
        //        //Arrange
        //        var sc = _searcher.CreateCriteria("content", SearchCriteria.BooleanOperation.Or);

        //        //set the rewrite method before adding queries
        //        var lsc = (LuceneSearchCriteria)sc;
        //        lsc.QueryParser.MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE;

        //        sc = sc.NodeName("lo".MultipleCharacterWildcard())
        //            .Or().Field("headerText", "lo".MultipleCharacterWildcard())
        //            .Or().Field("bodyText", "lo".MultipleCharacterWildcard()).Compile();

        //        //Act

        //        Assert.Throws<BooleanQuery.TooManyClauses>(() => _searcher.Search(sc));
                
        //    }
        //    finally
        //    {
        //        //reset
        //        BooleanQuery.MaxClauseCount = 1024;
        //    }      
        //}
*/
    }
}