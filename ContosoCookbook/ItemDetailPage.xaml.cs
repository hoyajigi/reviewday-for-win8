﻿using ContosoCookbook.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232
using Windows.ApplicationModel.DataTransfer;
using System.Text;
using Windows.Storage.Streams;


namespace ContosoCookbook
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : ContosoCookbook.Common.LayoutAwarePage
    {
        private static TypedEventHandler<DataTransferManager, DataRequestedEventArgs> _handler;
        private RecipeDataItem _item; // Recipe currently displayed

        public ItemDetailPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var item = RecipeDataSource.GetItem((String)navigationParameter); 
            this.DefaultViewModel["Group"] = item.Group;
            this.DefaultViewModel["Items"] = item.Group.Items;
//            if(flipView1.Visibility==Visibility.Visible)
                this.flipView1.SelectedItem = item;
//            else
//                this.flipView2.SelectedItem = item;
            this._item = item;

            // Register handler for DataRequested events for sharing
            if (_handler != null)
                DataTransferManager.GetForCurrentView().DataRequested -= _handler;

            _handler = new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(OnDataRequested);
            DataTransferManager.GetForCurrentView().DataRequested += _handler;
           /* if (item.Directions == "")
            {
                flipView1.Visibility = Visibility.Collapsed;
                flipView2.Visibility = Visibility.Visible;
            }
            else
            {*/
                flipView1.Visibility = Visibility.Visible;
                flipView2.Visibility = Visibility.Collapsed;
           // }
        }
        void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
{
var request = args.Request;
request.Data.Properties.Title = _item.Title;
request.Data.Properties.Description = "Recipe ingredients and directions";

    // Share recipe text
StringBuilder builder = new StringBuilder();
builder.Append("INGREDIENTS\r\n");


builder.Append(_item.Ingredients);
builder.Append("\r\n");

builder.Append("\r\nDIRECTIONS\r\n");
builder.Append(_item.Directions);

request.Data.SetText(builder.ToString());
// Share recipe image
string url = _item.GetImageUri();
if (!url.StartsWith("http://"))
    url = "ms-appx:///" + url;

var uri = new Uri(url);
var reference = RandomAccessStreamReference.CreateFromUri(uri);
request.Data.Properties.Thumbnail = reference;
request.Data.SetBitmap(reference);

}

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            if (flipView1.Visibility == Visibility.Visible)
            {
                var selectedItem = (RecipeDataItem)this.flipView1.SelectedItem; pageState["SelectedItem"] = selectedItem.UniqueId;
            }
            else
            {
                var selectedItem = (RecipeDataItem)this.flipView2.SelectedItem; pageState["SelectedItem"] = selectedItem.UniqueId;
            }
        }
        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._item = (sender as FlipView).SelectedItem as RecipeDataItem;
        }
    }
}
