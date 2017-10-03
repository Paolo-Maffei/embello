\ configuration settings for SMA solar

\ this is the string returned during +INQ in command mode
\ once paired, this string is also returned by the +MRAD command
\ the format is XXXX:YY:ZZZZZZ, with leading 0's omitted and colons iso commas
\ : myaddr-str ( -- a n )  s" 80,25,A4EC14" ;
: myaddr-str ( -- a n )  s" 80,25,A50520" ;

\ this is the SMA's address, returned by +ADDR in command mode
\ it's received as "2013:10:220486" and needs to be entered here as hex
$2013 $10220486 2variable hisAddr-bin
