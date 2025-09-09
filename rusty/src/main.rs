use axum::{response::IntoResponse, routing::get, Json, Router};

#[tokio::main]
async fn main() {
    let app = Router::new().route("/hello", get(handler));

    let port = std::env::var("PORT").unwrap_or_else(|_| "3000".to_string());
    let addr = format!("127.0.0.1:{}", port);
    let listener = tokio::net::TcpListener::bind(&addr)
        .await
        .unwrap();
    println!("listening on {}", listener.local_addr().unwrap());
    axum::serve(listener, app).await.unwrap();
}

#[derive(serde::Serialize)]
struct HandlerResponse {
    message: String,
}

async fn handler() -> impl IntoResponse {
    Json(HandlerResponse {
        message: "Hello, World!".into(),
    })
}
