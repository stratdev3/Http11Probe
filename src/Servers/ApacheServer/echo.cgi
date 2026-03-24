#!/bin/sh
printf 'Content-Type: text/plain\r\n\r\n'
env | grep '^HTTP_' | while IFS='=' read -r key value; do
    name=$(echo "$key" | sed 's/^HTTP_//;s/_/-/g')
    printf '%s: %s\n' "$name" "$value"
done
if [ -n "$CONTENT_TYPE" ]; then
    printf 'CONTENT-TYPE: %s\n' "$CONTENT_TYPE"
fi
if [ -n "$CONTENT_LENGTH" ]; then
    printf 'CONTENT-LENGTH: %s\n' "$CONTENT_LENGTH"
fi
