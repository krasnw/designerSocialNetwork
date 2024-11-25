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
    meta: { requiresAuth: true },
  },
  {
    path: "/conversations",
    name: "conversations",
    component: () => import("@/pages/conversations/Conversations.vue"),
    meta: { requiresAuth: true },
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
    meta: { requiresAuth: true },
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
    meta: { requiresGuest: true },
  },
  {
    path: "/register",
    name: "register",
    component: () => import("@/pages/login-page/RegisterPage.vue"),
    meta: { requiresGuest: true },
  },
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const isAuthenticated = !!localStorage.getItem("JWT");

  if (to.meta.requiresAuth && !isAuthenticated) {
    // Если требуется авторизация, но пользователь не авторизован
    next("/login");
  } else if (to.meta.requiresGuest && isAuthenticated) {
    // Если страница только для гостей, а пользователь авторизован
    next("/feed");
  } else {
    next();
  }
});
