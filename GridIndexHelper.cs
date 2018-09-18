using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer
{
    /// <summary>
    /// Общая логика проставления индексов
    /// </summary>
    static class GridIndexHelper
    {
        public static void SetIndexes<T>(IEnumerable<T> indexableCollection) 
            where T : IIndexable
        {
            //если коллекция не содержит элементов, дальнейшие действия бессмыслены
            if (indexableCollection == null || indexableCollection.FirstOrDefault() == null)
                return;

            int index = 0;
            foreach (var item in indexableCollection)
            {
                item?.SetIndex(index);
                index++;
            }
        }
    }
}
