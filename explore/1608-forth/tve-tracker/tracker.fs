\ application setup and main loop
\ assumes that the GPS is connected to usart2 on PA2&PA3

: TESTS ;
include ../flib/any/testing.fs

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
1 constant rate   \ seconds between readings

: s>uart2 ( c-addr len -- ) \ send string to uart2
  0 ?do dup i + c@ uart2-emit loop drop ;

: s>gps ( c-addr len -- ) \ send command string to GPS
  s" PMTK"  s>uart2 s>uart2 13 uart2-emit 10 uart2-emit
  ;

: gps? \ print GPS info
  s" 605,*31" s>gps
  ;

: fwd
  begin uart2-key? if uart2-key emit then key? until ." OK" ;

\ The GPS NMEA stanzas have the form: $GPRMC,A,34,30*23<cr><lf>
\ The hex number after the * is the xor of all chars between the leading $ and the *

[ifdef] TESTS
  \ for tests we read input from a pre-filled buffer instead of reading from uart
  0 variable inptr
  : get-key inptr dup @ dup c@ swap 1+ rot ! ;
[else]
  \ when not testing read from uart
  : get-key uart2-key ;
[then]

10 constant LF
13 constant RET
char * constant STAR
char $ constant DOLLAR
200 buffer: line
0 variable line-ptr

: skip-line ( -- ) begin get-key LF = until ;
: grab-$ ( -- f ) begin get-key DOLLAR <> while skip-line repeat ;
: good-char ( c -- c f ) dup STAR <> over LF <> and ;
: save-char ( i c -- c i++ ) swap 2dup line + c! 1+ ;
: hex-char ( c - u ) 48 - dup 9 > if 7 - dup 15 > if 32 - then then $f and ;

: check-tail ( sum i -- i )
  get-key hex-char 4 lshift get-key hex-char +
  rot <> if drop 0 then
  ;

: read-line ( -- n ) \ reads line, checks sum, strips trailer, returns length, 0 if not OK
  grab-$ DOLLAR line c!
  0 1 ( sum i )
  begin
    get-key good-char while
    save-char ( sum c i )
    -rot xor swap \ save and update checksum
  repeat ( exit: sum i c )
  STAR = if check-tail skip-line else 2drop 0 then
  ;

44 constant COMMA
: null-terminate ( c-addr len -- ) + 0 swap c! ;
: find-comma ( c-addr -- i )
  0 begin ( c-addr i )
    2dup + c@
    dup 0= swap COMMA = or if swap drop exit then
    1+
  again
  ;
: next ( c-addr -- c-addr2 c-addr i true | c-addr false )
  dup find-comma
  dup if ( c-addr i )
    2dup + dup c@ COMMA = 1 and + \ advance c-addr if we found a comma
    -rot dup
  then \ advance c-addr if we found a comma
  ;

: $GPRMC ( c-addr -- ) ." got $GPRMC " dup . cr
  next if ." UTC:" type space then
  next if ." STATUS:" type space then
  next if ." LAT:" type space then
  next if ." NS:" type space then
  next if ." LON:" type space then
  next if ." EW:" type space then
  drop
  ;

: dispatch-line ( -- )
  line find-comma ( i )
  dup line swap find
  if ( i a-addr ) swap line + 1+ swap execute
  else drop line swap ." skipping " type cr then
  ;

: do-line ( -- )
  read-line dup if
    line swap null-terminate
    dispatch-line
  else drop then
  ;

16MHz 1000 systick-hz
uart2-init

[ifdef] TESTS
  200 buffer: inbuf

  : add-crlf ( c-addr len -- c-addr len+2 )
    2dup + RET swap c! 1+
    2dup + LF swap c! 1+
    ;

  : set-input ( c-addr len -- )
    inbuf -rot buffer-cpy
    add-crlf
    drop inptr !
    ;

  \ test hex-char
  char 0 hex-char 0 =always
  char 9 hex-char 9 =always
  char A hex-char 10 =always
  char F hex-char 15 =always
  char a hex-char 10 =always
  char f hex-char 15 =always

  \ test find-comma
  : s s" abcd,ef" ; s drop find-comma 4 =always
  : s s" abcd," ; s drop find-comma 4 =always
  : s s" abcdX" ; s 1- null-terminate  s drop find-comma 4 =always

  \ test next
  : s s" abcd,ef" ; s drop next
    4 =always  4 =always  s drop = always  s drop 5 + =always
  : s s" abcd" ; line s buffer-cpy 2dup null-terminate  line next
    4 =always 4 =always line = always line 4 + =always
  : s s" abcd,ef" ; line s buffer-cpy drop 4 + 0 swap c!  line next
    4 =always  4 =always  line =always  dup line 4 + =always
    next
    0 =always  line 4 + = always

  : s s" $PMTK001,604,3*32" ; s set-input read-line 14 =always \ correct chksum
  : s s" $PMTK001,604,3*33" ; s set-input read-line  0 =always \ bad chksum
  : s s" $PMTK001,604,3" ; s set-input read-line  0 =always \ no chksum
  : s s" X$PMTK..$PMTK001,604,3*32" ; s set-input 
    inbuf 6 + dup RET swap c! 1+ LF swap c!
    read-line  14 =always \ skip start

  : s s" $PMTK001,604,3*32" ; s set-input do-line 
  : s s" $GPRMC,060317.000,A,3429.9581,N,11949.0944,W,0.36,209.30,090117,,,D*7D" ;
    s set-input do-line

  : s s" $GPRMC,060317.000,A,3429.9581" ;
    line s buffer-cpy  2dup null-terminate
    line 7 + $GPRMC

  test-summary

[else]
  : main begin do-line key? until ;
[then]


