﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;
using Windows.Data.Json;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.



namespace ContosoCookbook.Data
{
    /// <summary>
    /// Base class for <see cref="RecipeDataItem"/> and <see cref="RecipeDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class RecipeDataCommon : ContosoCookbook.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public RecipeDataCommon(String uniqueId, String title, String shortTitle, String imagePath)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._shortTitle = shortTitle;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _shortTitle = string.Empty;
        public string ShortTitle
        {
            get { return this._shortTitle; }
            set { this.SetProperty(ref this._shortTitle, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(RecipeDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public string GetImageUri()
        {
            return _imagePath;
        }
    }

    /// <summary>
    /// Recipe item data model.
    /// </summary>
    public class RecipeDataItem : RecipeDataCommon
    {
        public RecipeDataItem()
            : base(String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }
        
        public RecipeDataItem(String uniqueId, String title, String shortTitle, String imagePath, int preptime, String directions, String ingredients, RecipeDataGroup group)
            : base(uniqueId, title, shortTitle, imagePath)
        {
            this._preptime = preptime;
            this._directions = directions;
            this._ingredients = ingredients;
            this._group = group;
        }

        private int _preptime = 0;
        public int PrepTime
        {
            get { return this._preptime; }
            set { this.SetProperty(ref this._preptime, value); }
        }
        
        private string _directions = string.Empty;
        public string Directions
        {
            get { return this._directions; }
            set { this.SetProperty(ref this._directions, value); }
        }

        private String _ingredients;
        public String Ingredients
        {
            get { return this._ingredients; }
            set { this.SetProperty(ref this._ingredients, value); }
        }
    
        private RecipeDataGroup _group;
        public RecipeDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Recipe group data model.
    /// </summary>
    public class RecipeDataGroup : RecipeDataCommon
    {
        public RecipeDataGroup()
            : base(String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }
        
        public RecipeDataGroup(String uniqueId, String title, String shortTitle, String imagePath, String description)
            : base(uniqueId, title, shortTitle, imagePath)
        {
        }

        private ObservableCollection<RecipeDataItem> _items = new ObservableCollection<RecipeDataItem>();
        public ObservableCollection<RecipeDataItem> Items
        {
            get { return this._items; }
        }

        public IEnumerable<RecipeDataItem> TopItems
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed
            get { return this._items.Take(12); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items.
    /// </summary>
    public sealed class RecipeDataSource
    {
        private static RecipeDataSource _recipeDataSource = new RecipeDataSource();

        private ObservableCollection<RecipeDataGroup> _allGroups = new ObservableCollection<RecipeDataGroup>();
        public ObservableCollection<RecipeDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<RecipeDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            if(_recipeDataSource!=null)
                return _recipeDataSource.AllGroups;
            return null;
        }

        public static RecipeDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _recipeDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static RecipeDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _recipeDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async System.Threading.Tasks.Task LoadRemoteDataAsync()
        {

            if (_recipeDataSource != null && _recipeDataSource.AllGroups.Count > 0) return;

            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile==null||profile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
            {
                
            }
            else
            {
                // Retrieve Recipe data from Azure
                try
                {
                    var client = new HttpClient();
                    client.MaxResponseContentBufferSize = 1024 * 1024; // Read up to 1 MB of data
                    var response = await client.GetAsync(new Uri("http://me2day.net/api/get_best_contents.json?&akey=3345257cb3f6681909994ea2c0566e80&asig=MTMzOTE2NDY1MiQkYnlidWFtLnEkJDYzZTVlM2EwOWUyYmI5M2Q0OGU4ZjlmNzA4ZjUzYjMz&locale=ko-KR"));
                    var result = await response.Content.ReadAsStringAsync();

                    // Parse the JSON Recipe data
                    var Recipes = JsonArray.Parse(result.Substring(12, result.Length - 13));

                    // Convert the JSON objects into RecipeDataItems and RecipeDataGroups
                    await CreateRecipesAndRecipeGroups(Recipes);
                }
                catch (Exception ex)
                {
                    _recipeDataSource = null;
                }
            }
        }

        public static async System.Threading.Tasks.Task LoadLocalDataAsync()
        {
            // Retrieve Recipe data from Recipes.txt
            var file = await Package.Current.InstalledLocation.GetFileAsync("Data\\Recipes.txt");
            var stream = await file.OpenReadAsync();
            var input = stream.GetInputStreamAt(0);
            var reader = new DataReader(input);
            uint count = await reader.LoadAsync((uint)stream.Size);
            var result = reader.ReadString(count);

            // Parse the JSON Recipe data
            var Recipes = JsonArray.Parse(result.Substring(1, result.Length - 1));
            //var Recipes = JsonArray.Parse(result);

            // Convert the JSON objects into RecipeDataItems and RecipeDataGroups
            await CreateRecipesAndRecipeGroups(Recipes);

        }

        private static async System.Threading.Tasks.Task CreateRecipesAndRecipeGroups(JsonArray array)
        {
            try
            {
                foreach (var item in array)
                {
                    var obj = item.GetObject();
                    RecipeDataItem Recipe = new RecipeDataItem();
                    RecipeDataGroup group = null;

                    foreach (var key in obj.Keys)
                    {
                        IJsonValue val;
                        if (!obj.TryGetValue(key, out val))
                            continue;

                        switch (key)
                        {
                            case "identifier":
                                Recipe.UniqueId = val.GetString();
                                var client = new HttpClient();
                                client.MaxResponseContentBufferSize = 1024 * 1024; // Read up to 1 MB of data
                                var response = await client.GetAsync(new Uri("http://me2day.net/api/get_content.json?domain=" + Recipe.Group.Title + "&identifier=" + Convert.ToInt32(val.GetString()) + "&akey=3345257cb3f6681909994ea2c0566e80&asig=MTMzOTE2NDY1MiQkYnlidWFtLnEkJDYzZTVlM2EwOWUyYmI5M2Q0OGU4ZjlmNzA4ZjUzYjMz&locale=ko-KR"));
                                var result = await response.Content.ReadAsStringAsync();

                                // Parse the JSON Recipe data
                                var Recipes = JsonObject.Parse(result/*.Substring(12, result.Length - 13)*/);
                                foreach (var item1 in Recipes)
                                {
                                    if (item1.Key == "detail")
                                    {

                                        //var obj1 = item1.GetObject();
                                        var obj1 = item1.Value.GetObject();
                                        foreach (var key1 in obj1)
                                        {

                                            IJsonValue val1;
                                            /*
                                               if (!obj1.TryGetValue(key1, out val1))
                                                       continue;
                                            */
                                            val1 = key1.Value;
                                            switch (key1.Key)
                                            {
                                                case "title":
                                                    Recipe.Title = val1.GetString();
                                                    break;
                                                case "artist":
                                                    Recipe.ShortTitle = val1.GetString();
                                                    break;
                                                case "cast":
                                                    Recipe.ShortTitle = val1.GetString();
                                                    break;
                                                case "author":
                                                    Recipe.ShortTitle = val1.GetString();
                                                    break;
                                                //  case "rate":
                                                //     Recipe.PrepTime = Convert.ToInt32(val1.GetString());
                                                //    break;
                                                case "description":
                                                    Recipe.Directions = val1.GetString();
                                                    Recipe.Directions = Recipe.Directions.Replace("&amp;", "&");

                                                    Recipe.Directions = Recipe.Directions.Replace("&gt;", ">");
                                                    Recipe.Directions = Recipe.Directions.Replace("&lt;", "<");

                                                    break;

                                                case "image_url":
                                                    Recipe.SetImage(val1.GetString());
                                                    break;
                                            }
                                        }
                                    }
                                }
                                break;
                            case "domain":
                                string groupKey = val.GetString();


                                group = _recipeDataSource.AllGroups.FirstOrDefault(c => c.Title.Equals(groupKey));

                                if (group == null)
                                    group = CreateRecipeGroup(groupKey);

                                Recipe.Group = group;
                                break;
                        }
                    }

                    if (group != null)
                    {
                        Recipe.Ingredients = "";
                        group.Items.Add(Recipe);

                    }
                }
                _recipeDataSource.AllGroups.FirstOrDefault(c => c.Title.Equals("movie")).Title = "영화";
                _recipeDataSource.AllGroups.FirstOrDefault(c => c.Title.Equals("music_album")).Title = "음반";
                _recipeDataSource.AllGroups.FirstOrDefault(c => c.Title.Equals("book")).Title = "책";
            }
            catch (Exception ex)
            {
                _recipeDataSource = null;
            }
        }
        
        private static RecipeDataGroup CreateRecipeGroup(string obj)
        {
            RecipeDataGroup group = new RecipeDataGroup();

            group.Title = obj;
            group.UniqueId = new Random().Next().ToString();
            
            _recipeDataSource.AllGroups.Add(group);
            return group;
        }
    }
}
