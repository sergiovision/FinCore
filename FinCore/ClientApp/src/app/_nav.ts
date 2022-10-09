export const navItems = [
  {
    name: 'Dashboard',
    url: '/dashboard',
    icon: 'icon-speedometer'
  },
  {
    name: 'Crypto',
    url: '/crypto',
    icon: 'icon-speedometer',
  },
  {
    title: true,
    name: 'Components'
  },
  {
    name: 'Logs',
    url: '/logs',
    icon: 'fa fa-code',
  },
  /* {
    name: 'Forms',
    url: '/forms',
    icon: 'icon-note',
    children: [
      {
        name: 'Basic Forms',
        url: '/forms/basic-forms',
        icon: 'icon-note'
      },
      {
        name: 'Advanced',
        url: '/forms/advanced-forms',
        icon: 'icon-note',
        badge: {
          variant: 'danger',
          text: 'PRO'
        }
      },
      {
        name: 'Validation',
        url: '/forms/validation-forms',
        icon: 'icon-note',
        badge: {
          variant: 'danger',
          text: 'PRO'
        }
      },
    ]
  },
  {
    name: 'Notifications',
    url: '/notifications',
    icon: 'icon-bell',
    children: [
      {
        name: 'Alerts',
        url: '/notifications/alerts',
        icon: 'icon-bell'
      },
      {
        name: 'Badges',
        url: '/notifications/badges',
        icon: 'icon-bell'
      },
      {
        name: 'Modals',
        url: '/notifications/modals',
        icon: 'icon-bell'
      }
    ]
  }, */
  {
    name: 'Statistics',
    url: '/stat',
    icon: 'icon-pie-chart',
    children: [
      {
        name: 'Instruments',
        url: '/stat/symbols',
        icon: 'icon-chart',
      },
      {
        name: 'Performance',
        url: '/stat/performance',
        icon: 'icon-credit-card',
      },
      {
        name: 'Capital',
        url: '/stat/capital',
        icon: 'icon-pie-chart',
      },
      {
        name: 'Investments',
        url: '/stat/investments',
        icon: 'icon-pie-chart',
      }
    ]
  },
  {
    name: 'Tables',
    url: '/tables',
    icon: 'icon-briefcase',
    children: [
      {
        name: 'Wallets',
        url: '/tables/wallets',
        icon: 'icon-wallet'
      },
      {
        name: 'Metasymbols',
        url: '/tables/metasymbols',
        icon: 'icon-star'
      },
      {
        name: 'Rates',
        url: '/tables/rates',
        icon: 'icon-note'
      },
      {
        name: 'Jobs',
        url: '/tables/jobs',
        icon: 'icon-settings'
      },
      {
        name: 'Deals',
        url: '/tables/deals',
        icon: 'icon-graph'
      },
      {
        name: 'Settings',
        url: '/tables/settings',
        icon: 'icon-grid'
      },
      {
        name: 'Logins',
        url: '/tables/person',
        icon: 'icon-login'
      }

    ]
  },
  {
    name: 'Documentation',
    url: '/doc',
    icon: 'icon-question'
  }
  /* ,
  {
    name: 'Chart',
    url: '/chart',
    icon: 'icon-graph'
  }
 */
];
