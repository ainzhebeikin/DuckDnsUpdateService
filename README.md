DuckDnsUpdateService
====================

Updates duckdns.org account's domain with IP address as it changes

Actually, duckdns.org offers plenty of scripts and GUIs doing similar thing.
The difference is this service does not poll, but performs an update only
when monitored network interface's IP address does really change.

To use this as a Windows Service do the following:
1.  Build solution
2.  (optional) Copy built artifacts to Program Files subfolder
3.  Run exe with -i parameter -> this will create a DuckDnsUpdateService
    configured to auto-start with system
4.  Put domain name and token into .config file. Do not worry, nobady can
    steal your token cause the service will encrypt it right in the .config file
    upon first start.
    Note: make sure user account under which the service encrypts your config matches
    one used to run the service. If you, for example, put your config and start exe
    from command line (with your user account), it will encrypt config using your
    account's key. If you then start service under LocalSystem account (the default),
    it won't be able to decrypt. In case of mess, just re-enter your unencrypted data.
5.  Enjoy

Looking for a log file: by default it's in hidden folder C:\ProgramData\DuckDnsUpdateService. Use NLog.config to tune log file.

Finally, you can run exe as a regular program using --console switch.
