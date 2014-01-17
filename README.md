# IIS Restart issue

There's some code in katana that is used to detect IIS/System.Web shut downs in various cases:
- App Domain Restart (e.g. config change or code change)
- App Pool Recycle (IIS manager)
- Stopping/Restarting the website in Inetmgr

## Problems with IIS WebSite restart/stop

It seems like this doesn't work if there are long running requests pending.