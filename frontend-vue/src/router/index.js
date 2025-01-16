// src/router/index.js
import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "/",
    redirect: "/feed",
  },
  {
    path: "/profile",
    redirect: "/profile/me",
  },
  {
    path: "/:username",
    redirect: "/:username/portfolio",
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
    path: "/profile/me",
    name: "myProfile",
    component: () => import("@/pages/profile/Portfolio.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/profile/me/edit",
    name: "editProfile",
    component: () => import("@/pages/profile/EditProfile.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/:username/portfolio",
    name: "portfolio",
    component: () => import("@/pages/profile/Portfolio.vue"),
    props: true,
  },
  {
    path: "/:username/add-task",
    name: "addTask",
    component: () => import("@/pages/profile/AddRequest.vue"),
    meta: { requiresAuth: true },
    props: true,
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
  {
    path: "/error",
    name: "error",
    children: [
      {
        path: "400",
        name: "error400",
        component: () => import("@/pages/Errors/400.vue"),
      },
      {
        path: "401",
        name: "error401",
        component: () => import("@/pages/Errors/401.vue"),
      },
      {
        path: "403",
        name: "error403",
        component: () => import("@/pages/Errors/403.vue"),
      },
      {
        path: "404",
        name: "error404",
        component: () => import("@/pages/Errors/404.vue"),
      },
      {
        path: "500",
        name: "error500",
        component: () => import("@/pages/Errors/500.vue"),
      },
    ],
  },
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const isAuthenticated = !!localStorage.getItem("JWT");

  if (to.meta.requiresAuth && !isAuthenticated) {
    next("/login");
  } else if (to.meta.requiresGuest && isAuthenticated) {
    next("/feed");
  } else {
    next();
  }
});
