namespace ZiTools
{
	public interface ISeekController
	{
		void ChangeText(string text);
		void ChangeActiveCategory(Category category);
		void ChangeActiveSearchItem(ISearchItem searchItem);
		void AddFavourite(ISearchItem item);
		void RemoveFavourite(ISearchItem item);
	}
}