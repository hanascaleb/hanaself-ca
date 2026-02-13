echo off
mode com2 baud=9600 parity=n data=8
echo %1 >COM2
