# L²P News Ticker

This is an App for getting an overview, what happened in the last time in the L²P Course rooms of the User.


## Where to Download

If you just want to download the App for your smartphone, use these links:

* iOS: https://itunes.apple.com/de/app/l2p-newsticker/id1090748510?mt=8
* Windows Phone: https://www.microsoft.com/de-de/store/apps/l2p-newsticker/9nblggh4qdkx
* Android: https://play.google.com/store/apps/details?id=com.auxua.l2pnewsticker&hl=de

Or simply go to your App store and search for it.

## Configuration

In the Project you find the *ConfigSample.cs* - just add your ClientID in here to enable the Client to work.
This class comes from the original L²P Api Client and was moved to the shared project to add Persistent Storage Options.

## Dependencies

Besides the .NET Framework there are further dependencies (coming from the original API Client). These include :

 - Newtonsoft.JSON package - this is licensed under MIT license (see *RESTCalls.cs* where it is used) - If you do not want to use this for JSON-(De)Serialization, you might replace it.
 - System.net.http - this is an assembly of Microsoft, that is not part of the inner .NET Core but is available via NuGet. This package is used for the Http WebRequests for the REST-Calls
 
All of these dependencies are available as PCL enabling support for portability.

Further, the Projects are using Xamarin Forms to enable the App for WindowsPhone/Windows Mobile, iOS and Android
 
## Remarks

 - This App is made for Users of the L²P - Please do not ask for Accounts of L²P
 - If you find Bugs in the API, please report to CiL
 - By using the L2P API you have to commit to the RWTH/CiL/IT Center Guidelines for API usage and Privacy

## License

This piece of software is licensed under the 2-clause BSD License (see *LICENSE*) - so, it's Open Source and everyone is welcome to use it.