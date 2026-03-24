#!/bin/sh
printf 'Content-Type: text/plain\r\n\r\n'
if [ "$REQUEST_METHOD" = "POST" ] && [ "${CONTENT_LENGTH:-0}" -gt 0 ] 2>/dev/null; then
    head -c "$CONTENT_LENGTH"
else
    printf 'OK'
fi
