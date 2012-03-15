﻿/*
 * Copyright 2012 WildCard, LLC
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Nxdb.Node;
using Nxdb.Persistence.Attributes;

namespace Nxdb.Persistence
{
    internal class DefaultPersister : Persister
    {
        internal override void Fetch(Element element, object target, TypeCache typeCache, Cache cache)
        {
            if (target == null) return;

            // Get all values first so if something goes wrong we haven't started modifying the object
            List<KeyValuePair<MemberInfo, object>> values
                = new List<KeyValuePair<MemberInfo, object>>();
            foreach (KeyValuePair<MemberInfo, PersistentMemberAttribute> kvp
                in typeCache.PersistentMembers.Where(kvp => kvp.Value.Fetch))
            {
                Type memberType;
                object member = GetValue(kvp.Key, target, out memberType);
                TypeCache memberTypeCache = memberType != null ? cache.GetTypeCache(memberType) : null;
                object value = kvp.Value.FetchValue(element, member, memberTypeCache, cache);
                if(kvp.Value.Required && value == null) throw new Exception("Could not get required member.");
                values.Add(new KeyValuePair<MemberInfo, object>(kvp.Key, value));
            }

            // Now that all conversions have been succesfully performed, set the values
            foreach (KeyValuePair<MemberInfo, object> value in values)
            {
                SetValue(value.Key, target, value.Value);
            }
        }

        private class SerializedValue
        {
            public PersistentMemberAttribute PersistentMemberAttribute { get; private set; }
            public object Serialized { get; private set;}
            public object Member { get; private set; }
            public TypeCache MemberTypeCache { get; private set; }

            public SerializedValue(PersistentMemberAttribute persistentMemberAttribute,
                object serialized, object member, TypeCache memberTypeCache)
            {
                PersistentMemberAttribute = persistentMemberAttribute;
                Serialized = serialized;
                Member = member;
                MemberTypeCache = memberTypeCache;
            }
        }

        internal override object Serialize(object source, TypeCache typeCache, Cache cache)
        {
            if (source == null) return null; 
            
            List<SerializedValue> values = new List<SerializedValue>();
            foreach (KeyValuePair<MemberInfo, PersistentMemberAttribute> kvp
                in typeCache.PersistentMembers.Where(kvp => kvp.Value.Store))
            {
                Type memberType;
                object member = GetValue(kvp.Key, source, out memberType);
                TypeCache memberTypeCache = memberType != null ? cache.GetTypeCache(memberType) : null;
                object serialized = kvp.Value.SerializeValue(member, memberTypeCache, cache);
                values.Add(new SerializedValue(kvp.Value, serialized, member, memberTypeCache));
            }
            return values;
        }

        // This scans over all instance fields in the object and uses a TypeConverter to convert them to string
        // If any fields cannot be converted an exception is thrown because the entire state was not stored
        internal override void Store(Element element, object serialized, object source, TypeCache typeCache, Cache cache)
        {
            if (source == null || serialized == null) return;
            foreach (SerializedValue value in (List<SerializedValue>)serialized)
            {
                value.PersistentMemberAttribute.StoreValue(element, value.Serialized,
                    value.Member, value.MemberTypeCache, cache);
            }
        }

        private object GetValue(MemberInfo memberInfo, object member, out Type type)
        {
            FieldInfo fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                type = fieldInfo.FieldType;
                return fieldInfo.GetValue(member);
            }
            
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if(propertyInfo != null)
            {
                type = propertyInfo.PropertyType;
                return propertyInfo.GetValue(member, null);
            }

            throw new Exception("Unexpected MemberInfo type.");
        }

        private void SetValue(MemberInfo memberInfo, object member, object value)
        {
            FieldInfo fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(member, value);
            }
            else
            {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                if(propertyInfo != null)
                {
                    propertyInfo.SetValue(member, value, null);
                }
            }
        }

        internal static Type GetMemberType(MemberInfo memberInfo)
        {
            FieldInfo fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }

            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                return propertyInfo.PropertyType;
            }

            throw new Exception("Unexpected MemberInfo type.");
        }
    }
}
