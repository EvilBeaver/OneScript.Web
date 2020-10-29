/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;

namespace OneScript.WebHost.Infrastructure
{
    public delegate object AnnotationMapper(AnnotationDefinition annotation);

    class AnnotationAttributeMapper
    {
        private readonly IndexedNameValueCollection<AnnotationMapper> _mappers = new IndexedNameValueCollection<AnnotationMapper>();
        
        public void AddMapper(string token, AnnotationMapper mapper)
        {
            AddMapper(token, null, mapper);
        }

        public void AddMapper(string token, string alias, AnnotationMapper mapper)
        {
            _mappers.Add(mapper, token);
            if(alias != null)
                _mappers.AddName(_mappers.IndexOf(token), alias);
        }

        public object Get(AnnotationDefinition annotation)
        {
            var found = _mappers.TryGetValue(annotation.Name, out var mapper);
            if (found)
                return mapper(annotation);

            return null;
        }

        
    }
}
