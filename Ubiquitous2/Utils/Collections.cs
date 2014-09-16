using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Utils
{
    public static class Collections
    {
        public static void RemoveAll<T>( this ObservableCollection<T> list, Func<T,bool> match  ) where T:class
        {
            var removeItem = list.With(x => list)
                .With(x => list.FirstOrDefault(match));
                
            if (removeItem != null)
                list.Remove(removeItem);
        }

    }
}
