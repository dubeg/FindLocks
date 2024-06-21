# README
2021-06-10 DUBEG:
Simple utility wrapping Sysinternal's Handle.exe.
It merely simplifies its output to a list of tuple (User: Process).

I use it within a powershell function (in my $PROFILE) 
to find out who's locking either the Acomba company RENDAC or SAJB,
whenever people from accounting want to obtain exclusive access 
to either of them.

It might either be an Acomba or a Servicentre process.
Servicentre locks Aco files via the AcoSDK.

It's regularly useful, around the day 10 of a month, to close the previous month.