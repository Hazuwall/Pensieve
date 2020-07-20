using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Pensieve
{
    public static class NavJournalHelper
    {
        /// <summary>
        /// Изменить параметры последних элементов в стеке
        /// </summary>
        /// <param name="stack">Стек навигации</param>
        /// <param name="newParameters">Новые параметры в порядке углубления</param>
        public static void EditLastEntries(IList<PageStackEntry> stack, params object[] newParameters)
        {
            int depth = newParameters.Length;
            PageStackEntry[] newEntries = new PageStackEntry[depth];
            for (int i = 0; i < depth; i++) {
                PageStackEntry oldEntry = stack[stack.Count - 1 - i];
                newEntries[i] = new PageStackEntry(oldEntry.SourcePageType,
                    newParameters[i], oldEntry.NavigationTransitionInfo);
            }
            for (int i = 0; i < depth; i++)
                stack.RemoveAt(stack.Count - 1 - i);
            //Добавление изменённых записей начиная с самого глубокого
            for (int i = depth - 1; i >= 0; i--)
                stack.Add(newEntries[i]);
        }

        /// <summary>
        /// Перейти на страницу и изменить записи в журнале посещений так, как если бы был совершён переход назад
        /// </summary>
        /// <param name="Stack">Стек навигации</param>
        /// <param name="NewParameters">Новые параметры в порядке углубления</param>
        public static void FakeGoBack(this Frame frame, Type sourcePageType, object parameter)
        {
            frame.Navigate(sourcePageType, parameter);
            PageStackEntry entry = frame.BackStack.Last();
            frame.BackStack.Remove(entry);
            frame.ForwardStack.Add(entry);
        }

        /// <summary>
        /// Вернуться на последнюю страницу данного типа с полной очисткой истории посещений. Если страница отсутствует в журнале, то совершить новый переход
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="sourcePageType"></param>
        /// <returns>Найдена ли страница в истории</returns>
        public static bool AnchorToLastPageOfType(this Frame frame, Type sourcePageType)
        {
            bool isFound = false;
            //удаление всех записей, кроме последней требуемого типа
            for (int i = frame.BackStack.Count - 1; i >= 0; i--)
            {
                if (!isFound && frame.BackStack[i].SourcePageType == sourcePageType)
                    isFound = true;
                else
                    frame.BackStack.RemoveAt(i);
            }
            if (isFound)
                frame.GoBack();
            else
            {
                //если страница в журнале не найдена, то совершение прямого перехода
                frame.Navigate(sourcePageType);
                frame.BackStack.Clear();
            }
            frame.ForwardStack.Clear();
            return isFound;
        }
    }
}
