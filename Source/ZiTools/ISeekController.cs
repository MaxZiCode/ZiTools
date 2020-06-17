namespace ZiTools
{
	public interface ISeekController
	{
		void ChangeText(string text);
		void ChangeActiveCategory(ICategory category);
		void ChangeActiveSearchItem(ISearchItem searchItem);
		void AddFavourite(ISearchItem item);
		void RemoveFavourite(ISearchItem item);
	}
}