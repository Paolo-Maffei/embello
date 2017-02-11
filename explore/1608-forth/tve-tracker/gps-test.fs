\ tests for tracker.fs

: rf-send ;
\ include ../flib/any/buffers.fs
include ../flib/any/varint.fs
include ../flib/any/testing.fs

\ for tests we read input from a pre-filled buffer instead of reading from uart
0 variable inptr
: get-key inptr dup @ dup c@ swap 1+ rot ! ;

include gps.fs

16MHz 1000 systick-hz
uart2-init

200 buffer: inbuf

: add-crlf ( c-addr len -- c-addr len+2 )
  2dup + RET swap c! 1+
  2dup + LF swap c! 1+
  ;

: buffer-cpy ( dest-addr src-addr cnt -- dest-addr cnt )
  swap 2 pick 2 pick ( dst cnt src dst cnt )
  move
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

\ test s>int
: s s" 0" ; s s>int 0 =always
: s s" 1" ; s s>int 1 =always
: s s" 12" ; s s>int 12 =always
: s s" -1" ; s s>int -1 =always
: s s" -13" ; s s>int -13 =always
: s s" -6835" ; s s>int -6835 =always
: s s" -6835x" ; s s>int -6835 =always
: s s" " ; s s>int 0 =always
: s s" 65b" ; s s>int 65 =always

\ test s>fix
: s s" 0" ; line s buffer-cpy s>fix 0 0 =always-fix
: s s" 1" ; line s buffer-cpy s>fix 0 1 =always-fix
: s s" 128.5" ; line s buffer-cpy s>fix $80000000 128 =always-fix
: s s" -567.125" ; line s buffer-cpy s>fix $e0000000 -568 =always-fix

\ test find-comma
: s s" abcd,ef" ; s drop find-comma 4 =always
: s s" abcd," ; s drop find-comma 4 =always
: s s" abcdX" ; s 1- null-terminate  s drop find-comma 4 =always

\ test next
: s s" abcd,ef" ; s drop next
  4 =always  4 =always  s drop = always  s drop 5 + =always
: s s" abcd" ; line s buffer-cpy null-terminate  line next
  4 =always 4 =always line = always line 4 + =always
: s s" abcd,ef" ; line s buffer-cpy drop 4 + 0 swap c!  line next
  4 =always  4 =always  line =always  dup line 4 + =always
  next
  0 =always  line 4 + = always

\ test variable-length encoding of fields
: cpy buffer-cpy null-terminate line ;
: s s" 1234.5678,N," ; line s cpy char S f4d 12345678 =always drop
: s s" 1234.5674,N," ; line s cpy char S f4d 12345674 =always drop
: s s" 1234.5674,S" ; line s cpy char S f4d -12345674 =always drop
: s s" 1234.5674" ; line s cpy char S f4d 12345674 =always drop
: s s" " ; line s cpy char S f4d 0 =always drop

\ test read-line
: s s" $PMTK001,604,3*32" ; s set-input read-line 14 =always \ correct chksum
: s s" $PMTK001,604,3*33" ; s set-input read-line  0 =always \ bad chksum
: s s" $PMTK001,604,3" ; s set-input read-line  0 =always \ no chksum
: s s" X$PMTK..$PMTK001,604,3*32" ; s set-input 
  inbuf 6 + dup RET swap c! 1+ LF swap c!
  read-line  14 =always \ skip start

\ not real tests, produce sample output...
\  : s s" $PMTK001,604,3*32" ; s set-input do-line 
\  : s s" $GPRMC,060317.000,A,3429.9581,N,11949.0944,W,0.36,209.30,090117,,,D*7D" ;
\    s set-input do-line
\ 
\  : s s" $GPRMC,060317.000,A,3429.9581" ;
\    line s buffer-cpy  null-terminate
\    line 7 + $GPRMC

test-summary
