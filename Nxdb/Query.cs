﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.basex.query;
using org.basex.query.expr;
using org.basex.query.item;
using org.basex.query.iter;
using org.basex.util;

namespace Nxdb
{
    public class Query
    {
        private string _expression;
        private readonly Dictionary<string, Value> _namedCollections
            = new Dictionary<string, Value>();
        private Value _defaultCollection = null;
        private Value _initialContext = null;
        private readonly Dictionary<string, Value> _variables
            = new Dictionary<string, Value>();

        public Query(string expression)
        {
            Expression = expression;
        }

        public string Expression
        {
            get { return _expression; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == String.Empty) throw new ArgumentException("value");
                _expression = value;
            }
        }

        public void SetInitialContext(object value)
        {
            _initialContext = value.ToValue();
        }

        //name == null or String.Empty -> set the default (first) collection
        //name == text -> set a named collection
        //value == null -> clear the specified context/collection
        public void SetCollection(string name, object value)
        {
            Value v = value.ToValue();
            if(String.IsNullOrEmpty(name))
            {
                _defaultCollection = v;
            }
            else
            {
                if(v == null)
                {
                    _namedCollections.Remove(name);
                }
                else
                {
                    _namedCollections[name] = v;
                }
            }
        }

        //value == null -> clear variable
        public void SetVariable(string name, object value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (name == String.Empty) throw new ArgumentException("name");
            Value v = value.ToValue();
            if(v == null)
            {
                _variables.Remove(name);
            }
            else
            {
                _variables[name] = v;
            }
        }

        public IEnumerable<object> Evaluate()
        {
            //Create the query context
            QueryContext queryContext = new QueryContext(Database.Context);

            //Add variables
            foreach(KeyValuePair<string, Value> kvp in _variables)
            {
                queryContext.bind(kvp.Key, kvp.Value);
            }

            //Add default collection
            queryContext.resource.addCollection(_defaultCollection ?? Empty.SEQ, String.Empty);

            //Add named collections
            foreach(KeyValuePair<string, Value> kvp in _namedCollections)
            {
                queryContext.resource.addCollection(kvp.Value, kvp.Key);
            }

            //Set the initial context item
            queryContext.ctxItem = _initialContext;

            //Parse the expression
            queryContext.parse(_expression);

            //Compile the query
            queryContext.compile();

            //Get the iterator and return the results
            Iter iter = queryContext.iter();
            return new IterEnum(iter);

            //TODO: Check if the query contained update expressions, and if so run the same cleanup/optimize as Update (or maybe use an Update container)
        }

        public IList<object> GetList()
        {
            return new List<object>(Evaluate());
        }

        public IList<T> GetList<T>()
        {
            return new List<T>(Evaluate().OfType<T>());
        }
    }
}
