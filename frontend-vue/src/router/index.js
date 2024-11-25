// src/router/index.js
import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "/",
    redirect: "/feed",
  },
  {
    path: "/feed",
    name: "feed",
    component: () => import("@/pages/feed/Feed.vue"),
  },
  {
    path: "/add-post",
    name: "addPost",
    component: () => import("@/pages/upload-post/UploadPost.vue"),
  },
  {
    path: "/conversations",
    name: "conversations",
    component: () => import("@/pages/conversations/Conversations.vue"),
  },
  {
    path: "/ranking",
    name: "ranking",
    component: () => import("@/pages/ranking/Ranking.vue"),
  },
  {
    path: "/task-list",
    name: "taskList",
    component: () => import("@/pages/task-list/TaskList.vue"),
  },
  {
    path: "/settings",
    name: "settings",
    component: () => import("@/pages/settings/Settings.vue"),
  },
  {
    path: "/login",
    name: "login",
    component: () => import("@/pages/login-page/LoginPage.vue"),
  },
  {
    path: "/register",
    name: "register",
    component: () => import("@/pages/login-page/RegisterPage.vue"),
  },
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
});
