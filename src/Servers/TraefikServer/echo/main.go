package main

import (
	"fmt"
	"io"
	"net/http"
	"strings"
)

func main() {
	http.HandleFunc("/cookie", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "text/plain")
		raw := r.Header.Get("Cookie")
		for _, pair := range strings.Split(raw, ";") {
			pair = strings.TrimLeft(pair, " ")
			if eq := strings.Index(pair, "="); eq > 0 {
				w.Write([]byte(pair[:eq] + "=" + pair[eq+1:] + "\n"))
			}
		}
	})

	http.HandleFunc("/echo", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "text/plain")
		for name, values := range r.Header {
			for _, v := range values {
				fmt.Fprintf(w, "%s: %s\n", name, v)
			}
		}
	})

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "text/plain")
		if r.Method == http.MethodPost {
			body, err := io.ReadAll(r.Body)
			if err != nil {
				http.Error(w, "Failed to read body", http.StatusBadRequest)
				return
			}
			defer r.Body.Close()
			w.Write(body)
			return
		}
		w.Write([]byte("OK"))
	})

	http.ListenAndServe(":9090", nil)
}
