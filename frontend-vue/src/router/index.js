// src/router/index.js
import { userService } from "@/services/user";
import { createRouter, createWebHistory } from "vue-router";

const checkIfOwnProfile = async (to, from, next) => {
  const isAuthenticated = !!localStorage.getItem("JWT");
  if (!isAuthenticated) {
    next();
    return;
  }

  try {
    const userData = await userService.getMyData();
    to.params.username === userData.username
      ? next({ name: "myProfile" })
      : next();
  } catch (error) {
    next();
  }
};

const routes = [
  {
    path: "/",
    redirect: "/feed",
  },
  {
    path: "/:pathMatch(.*)*",
    name: "notFound",
    component: () => import("@/pages/Errors/404.vue"),
  },
  {
    path: "/profile",
    redirect: "/profile/me",
  },
  {
    path: "/feed",
    name: "feed",
    component: () => import("@/pages/feed/Feed.vue"),
  },
  {
    path: "/post/:id",
    name: "post",
    component: () => import("@/pages/singlePost/SinglePost.vue"),
  },
  {
    path: "/post/protected/:hash",
    name: "protectedPost",
    component: () => import("@/pages/singlePost/SinglePost.vue"),
  },
  {
    path: "/add-post",
    name: "addPost",
    component: () => import("@/pages/upload-post/AddPost.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/conversations",
    name: "conversations",
    component: () => import("@/pages/conversations/Conversations.vue"),
    meta: { requiresAuth: true },
    children: [
      {
        path: ":username",
        name: "chat",
        component: () => import("@/pages/conversations/Chat.vue"),
      },
    ],
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
    path: "/report",
    name: "report",
    component: () => import("@/pages/report/ReportPage.vue"),
    beforeEnter: (to, from, next) => {
      const { username, postId } = to.query;
      if (!username && !postId) {
        next("/error/400");
      } else {
        next();
      }
    },
  },
  {
    path: "/profile/me",
    name: "myProfile",
    component: () => import("@/pages/profile/Portfolio.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/profile/rubies",
    name: "buyRubies",
    component: () => import("@/pages/profile/BuyRubies.vue"),
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
    beforeEnter: checkIfOwnProfile,
  },
  {
    path: "/:username/subscription",
    name: "subscription",
    component: () => import("@/pages/profile/Subscription.vue"),
    meta: { requiresAuth: true },
    props: true,
    beforeEnter: checkIfOwnProfile,
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
