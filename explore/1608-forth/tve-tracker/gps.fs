\ GPS driver
\ assumes that the GPS is connected to usart2 on PA2&PA3
\ TX packet format: 
\   UTC time (HHMMSS A/V flag (bit), lat (int32), lon (int32), knts*1E4 (uint32),
\   degrees*1E4 (int32), date (DDMMYY), mag var *1E4 (int32)

$8A constant GPRMC \ packet format

10 constant LF
13 constant RET
char * constant STAR
char $ constant DOLLAR
200 buffer: line

[ifndef] get-key : get-key uart2-key ; [then] \ redefined for tests
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
  then
  ;

: getc ( c-addr -- c-addr+1 digit ) dup 1+ swap c@ [char] 0 - ;
: digit? ( c -- c f ) dup 10 u< ;
: do-digit ( n c-addr digit -- n c-addr ) rot 10 * + swap ;
: sign? ( n x digit -- n x f ) -3 = 2 pick 0= and ; \ '-' - '0'
: do-sign ( fct n c-addr -- fct n c-addr ) rot -1 * -rot ;

: s>int ( c-addr len -- n ) \ parse int from string
  1 0 2swap ( fct value c-addr len )
  0 ?do ( fct value c-addr )
    getc digit? if do-digit else
         sign? if do-sign else
         leave then then
  loop drop * \ apply sign factor
  ;

: dot>comma ( c-addr len -- ) \ replace . by , in string
  0 ?do
    dup i + dup c@ [char] .
    = if [char] , swap c!  else drop then
  loop drop ;

: s>fix ( c-addr len -- df ) \ parse fixed-point from string
  2dup dot>comma
  number case
    0 of 0 0 endof
    1 of 0 swap endof
  endcase
  ;

: n>pkt ( c-addr -- c-addr ) \ send number
  next if s>int >+pkt then ;
: c>pkt ( c-addr -- c-addr ) \ send char
  next if 1 = if c@ else 0 then >+pkt then ;
: f3>pkt ( c-addr -- c-addr ) \ send number with 3 fractional digits
  next if s>fix 0,0005 d+ 0 1000 f* >+pkt drop then ;
: f4>pkt ( c-addr -- c-addr ) \ send number with 4 fractional digits
  next if s>fix 0,00005 d+ 0 10000 f* >+pkt drop then ;
: f4d ( c-addr c -- c-addr n ) \ prep number with 4 fractional digits and sign encoded by char
  swap
  next if
    s>fix 0,00005 d+ 0 10000 f* nip \ grab fractional number
  else 0 then
  -rot ( n c c-addr )
  next if ( n c c-addr2 c-addr i )
    1 = if ( n c c-addr2 c-addr )
      c@ rot = if ( n c-addr2 ) swap negate else swap then ( c-addr2 n )
    else drop nip swap then
  else nip swap then
  ;
: f4d>pkt ( c-addr c -- c-addr ) f4d >+pkt ;
: dmf>pkt ( c-addr c -- c-addr ) \ degrees minutes fractions: -11929.271 -> -11948.7850
  f4d dup 1000000 / 1000000 * dup -rot - 100 * 60 / + >+pkt ;

: <pkt ( -- ) pkt.buf pkt.ptr ! ;  \ start collecting values for the packet
: pkt> ( format -- c-addr len )    \ encode the collected values as RF packet
  ." PKT> " pkt.ptr @  begin  4 - dup @ . dup pkt.buf u<= until  drop dup . cr
  <v
    pkt.ptr @  begin  4 - dup @ >var  dup pkt.buf u<= until  drop
    hold
  v> ;

: $GPRMC ( c-addr -- c-addr len )
  \ ." got $GPRMC " dup . cr
  <pkt
    f3>pkt           \ UTC time
    c>pkt            \ A/V flag
    [char] S f4d>pkt \ latitude with N/S sign ( degrees minutes fractions )
    [char] W f4d>pkt \ latitude with E/W sign ( degrees minutes fractions )
\   [char] S dmf>pkt \ latitude with N/S sign ( degrees centi fractions )
\   [char] W dmf>pkt \ latitude with E/W sign ( degrees centi fractions )
    f4>pkt           \ speed knts
    f4>pkt           \ course made good
    n>pkt            \ date
    [char] W f4d>pkt \ magnetic variation E/W
    drop
  GPRMC pkt>
  ;

: dispatch-line ( -- 0 | c-addr len ) \ returns packet or 0 if none
  line find-comma ( i )
  dup line swap find
  if ( i a-addr )
    swap line + 1+ swap execute
  else 2drop 0 then
  ;

: gps-line ( -- 0 | c-addr length ) \ wait for a GPS line and return the info, or just 0
  read-line dup if
    ." RAW> " line over type cr
    line swap
    null-terminate
    dispatch-line
  then
  ;
